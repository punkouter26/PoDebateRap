using Azure;
using Azure.AI.OpenAI;
using Azure.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PoDebateRap.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using OpenAI.Chat;

namespace PoDebateRap.ServerApi.Services.AI
{
    public class AzureOpenAIService : IAzureOpenAIService
    {
        private readonly AzureOpenAIClient _openAIClient;
        private readonly ChatClient _chatClient;
        private readonly SpeechConfig? _speechConfig;
        private readonly ILogger<AzureOpenAIService> _logger;
        private readonly string _openAIDeploymentName;

        public AzureOpenAIService(IConfiguration configuration, ILogger<AzureOpenAIService> logger)
        {
            _logger = logger;

            // Azure OpenAI Configuration
            var openAIEndpoint = configuration["Azure:OpenAI:Endpoint"];
            var openAIApiKey = configuration["Azure:OpenAI:ApiKey"];
            _openAIDeploymentName = configuration["Azure:OpenAI:DeploymentName"] ?? "gpt-35-turbo";

            if (string.IsNullOrEmpty(openAIEndpoint) || string.IsNullOrEmpty(openAIApiKey))
            {
                _logger.LogError("Azure OpenAI endpoint or API key is not configured.");
                throw new ArgumentNullException("Azure OpenAI endpoint or API key is not configured.");
            }

            _openAIClient = new AzureOpenAIClient(new Uri(openAIEndpoint), new AzureKeyCredential(openAIApiKey));
            _chatClient = _openAIClient.GetChatClient(_openAIDeploymentName);
            _logger.LogInformation("Azure OpenAI client initialized with endpoint: {Endpoint}", openAIEndpoint);

            // Azure Speech Configuration
            var speechRegion = configuration["Azure:Speech:Region"];
            var speechSubscriptionKey = configuration["Azure:Speech:SubscriptionKey"];

            if (string.IsNullOrEmpty(speechRegion) || string.IsNullOrEmpty(speechSubscriptionKey))
            {
                _logger.LogWarning("Azure Speech region or subscription key is not configured. Text-to-Speech will be unavailable.");
                _speechConfig = null;
            }
            else
            {
                _speechConfig = SpeechConfig.FromSubscription(speechSubscriptionKey, speechRegion);
                _speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Riff16Khz16BitMonoPcm);
                _logger.LogInformation("Azure Speech client initialized for region: {Region}", speechRegion);
            }
        }

        public async Task<string> GenerateDebateTurnAsync(string prompt, int maxTokens, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Generating debate turn with prompt: {Prompt}", prompt);
            try
            {
                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage("You are a rap battle AI. Respond with creative and impactful rap lyrics."),
                    new UserChatMessage(prompt)
                };

                var options = new ChatCompletionOptions()
                {
                    Temperature = 0.7f,
                    MaxOutputTokenCount = maxTokens
                };

                var response = await _chatClient.CompleteChatAsync(messages, options, cancellationToken);
                string generatedText = response.Value.Content[0].Text;
                _logger.LogInformation("Generated debate turn: {Text}", generatedText);
                return generatedText;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Debate turn generation was canceled.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating debate turn from OpenAI.");
                throw;
            }
        }

        public async Task<JudgeDebateResponse> JudgeDebateAsync(string debateTranscript, string rapper1Name, string rapper2Name, string topic, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Judging debate for topic: {Topic}", topic);
            try
            {
                var systemPrompt = @"You are an impartial rap battle judge. Your task is to analyze the provided debate transcript between two rappers, " +
                                   $"{rapper1Name} and {rapper2Name}, on the topic of '{topic}'. " +
                                   "You must determine a winner based on lyrical skill, relevance to topic, creativity, and overall impact. " +
                                   "Provide a clear winner's name, a concise reasoning for your decision, and a numerical score (0-10) for each rapper in the following JSON format:\n" +
                                   "{\n" +
                                   "  \"WinnerName\": \"[Winner's Name]\",\n" +
                                   "  \"Reasoning\": \"[Your concise reasoning]\",\n" +
                                   "  \"Stats\": {\n" +
                                   "    \"Rapper1Score\": [Score for Rapper1],\n" +
                                   "    \"Rapper2Score\": [Score for Rapper2]\n" +
                                   "  }\n" +
                                   "}";

                var userPrompt = $"Debate Transcript:\n{debateTranscript}";

                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage(systemPrompt),
                    new UserChatMessage(userPrompt)
                };

                var options = new ChatCompletionOptions()
                {
                    Temperature = 0.5f,
                    ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat()
                };

                var response = await _chatClient.CompleteChatAsync(messages, options, cancellationToken);
                string jsonResponse = response.Value.Content[0].Text;
                _logger.LogInformation("Judge response (JSON): {JsonResponse}", jsonResponse);

                // Deserialize the JSON response
                var judgeResponse = JsonSerializer.Deserialize<JudgeDebateResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (judgeResponse == null)
                {
                    _logger.LogError("Failed to deserialize judge response: {JsonResponse}", jsonResponse);
                    return new JudgeDebateResponse { WinnerName = "Error Parsing", Reasoning = "Failed to parse judge's response.", Stats = new DebateStats() };
                }

                // Basic validation for scores
                if (judgeResponse.Stats == null)
                {
                    judgeResponse.Stats = new DebateStats();
                    _logger.LogWarning("Judge response missing 'Stats' object. Defaulting to 0 scores.");
                }

                return judgeResponse;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Debate judging was canceled.");
                throw;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON deserialization error during debate judging.");
                return new JudgeDebateResponse { WinnerName = "Error Parsing", Reasoning = $"JSON parsing error: {jsonEx.Message}", Stats = new DebateStats() };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error judging debate from OpenAI.");
                throw;
            }
        }

        public Task<byte[]> GenerateSpeechAsync(string text, string voiceName, CancellationToken cancellationToken)
        {
            // This service is a wrapper, the actual implementation is in TextToSpeechService.
            // This method will be removed in a future refactoring to separate concerns.
            throw new NotImplementedException("This method should be called on TextToSpeechService directly.");
        }
    }
}
