using Microsoft.Extensions.Logging;
using PoDebateRap.Shared.Models;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PoDebateRap.ServerApi.Services.AI;
using PoDebateRap.ServerApi.Services.Speech;
using PoDebateRap.ServerApi.Services.Data;
using Microsoft.Extensions.DependencyInjection;

namespace PoDebateRap.ServerApi.Services.Orchestration
{
    public class DebateOrchestrator : IDebateOrchestrator
    {
        private readonly ILogger<DebateOrchestrator> _logger;
        private readonly IServiceProvider _serviceProvider;

        private DebateState _currentState;
        public DebateState CurrentState => _currentState;

        public event Func<DebateState, Task> OnStateChangeAsync = null!;

        private CancellationTokenSource? _debateCancellationTokenSource;
        private TaskCompletionSource<bool> _audioPlaybackCompletionSource;

        private const int MaxDebateTurns = 10; // Total turns for the debate
        private const int MaxTokensPerTurn = 200; // Max tokens for AI generated rap
        private const string Rapper1Voice = "en-US-GuyNeural"; // Example voice for Rapper 1
        private const string Rapper2Voice = "en-US-JennyNeural"; // Example voice for Rapper 2

        public DebateOrchestrator(ILogger<DebateOrchestrator> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _currentState = new DebateState
            {
                Rapper1 = new Rapper(),
                Rapper2 = new Rapper(),
                Topic = new Topic(),
                CurrentTurnAudio = Array.Empty<byte>(),
                WinnerName = string.Empty,
                JudgeReasoning = string.Empty,
                Stats = new DebateStats(),
                ErrorMessage = string.Empty
            };
            _audioPlaybackCompletionSource = new TaskCompletionSource<bool>();
        }

        public void ResetDebate()
        {
            _logger.LogInformation("Resetting debate state.");
            _debateCancellationTokenSource?.Cancel(); // Cancel any ongoing debate
            _debateCancellationTokenSource?.Dispose();
            _debateCancellationTokenSource = null;

            _audioPlaybackCompletionSource?.TrySetResult(true); // Complete any pending audio playback
            _audioPlaybackCompletionSource = new TaskCompletionSource<bool>();

            _currentState = new DebateState
            {
                Rapper1 = new Rapper(),
                Rapper2 = new Rapper(),
                Topic = new Topic(),
                CurrentTurnAudio = Array.Empty<byte>(),
                WinnerName = string.Empty,
                JudgeReasoning = string.Empty,
                Stats = new DebateStats(),
                ErrorMessage = string.Empty
            };
            _ = NotifyStateChangeAsync(); // Fire-and-forget: Notify UI of reset
        }

        public async Task StartNewDebateAsync(Rapper rapper1, Rapper rapper2, Topic topic)
        {
            ResetDebate(); // Ensure a clean slate
            _debateCancellationTokenSource = new CancellationTokenSource();

            _currentState = new DebateState
            {
                Rapper1 = rapper1,
                Rapper2 = rapper2,
                Topic = topic,
                IsDebateInProgress = true,
                CurrentTurn = 0,
                TotalTurns = MaxDebateTurns,
                DebateTranscript = new StringBuilder(),
                IsRapper1Turn = true,
                CurrentTurnText = $"Get ready! Topic: '{topic.Title}'. {rapper1.Name} (Pro) vs {rapper2.Name} (Con). {rapper1.Name} starts...",
                IsGeneratingTurn = false, // Initial state, not generating yet
                CurrentTurnAudio = Array.Empty<byte>(),
                WinnerName = string.Empty,
                JudgeReasoning = string.Empty,
                Stats = new DebateStats(),
                ErrorMessage = string.Empty
            };
            await NotifyStateChangeAsync();

            // Initial audio for the intro text
            await GenerateAndPlaySpeechAsync(_currentState.CurrentTurnText, Rapper1Voice);
            // Do NOT wait for audio playback to complete here. The client will play the audio
            // and signal completion, allowing the API call to return immediately.
            _audioPlaybackCompletionSource = new TaskCompletionSource<bool>(); // Reset for next turn

            _logger.LogInformation("Debate started: {R1} vs {R2} on {Topic}", rapper1.Name, rapper2.Name, topic.Title);

            // Start the debate turns in a background task
            _ = Task.Run(() => RunDebateTurnsAsync(_debateCancellationTokenSource.Token));
        }

        private async Task RunDebateTurnsAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (_currentState.CurrentTurn < MaxDebateTurns && !cancellationToken.IsCancellationRequested)
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var openAiService = scope.ServiceProvider.GetRequiredService<IAzureOpenAIService>();
                        var ttsService = scope.ServiceProvider.GetRequiredService<ITextToSpeechService>();

                        _currentState.CurrentTurn++;
                        _currentState.IsGeneratingTurn = true;
                        // DON'T clear audio here - it causes the client to miss it during polling
                        // _currentState.CurrentTurnAudio = Array.Empty<byte>(); // Clear previous audio
                        _currentState.ErrorMessage = string.Empty; // Clear previous errors
                        await NotifyStateChangeAsync();

                        string currentRapperName = _currentState.IsRapper1Turn ? _currentState.Rapper1.Name : _currentState.Rapper2.Name;
                        string opponentRapperName = _currentState.IsRapper1Turn ? _currentState.Rapper2.Name : _currentState.Rapper1.Name;
                        string role = _currentState.IsRapper1Turn ? "Pro" : "Con";

                        string prompt = $"You are {currentRapperName} debating {opponentRapperName} on the topic '{_currentState.Topic.Title}'. " +
                                        $"Your role is {role}. This is turn {_currentState.CurrentTurn} of {MaxDebateTurns}. " +
                                        $"Current transcript:\n{_currentState.DebateTranscript.ToString()}\n" +
                                        $"Your rap:";

                        _logger.LogInformation("Generating turn {Turn} for {Rapper} ({Role}).", _currentState.CurrentTurn, currentRapperName, role);

                        try
                        {
                            _currentState.CurrentTurnText = await openAiService.GenerateDebateTurnAsync(prompt, MaxTokensPerTurn, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error generating turn {Turn} text.", _currentState.CurrentTurn);
                            _currentState.CurrentTurnText = $"Error generating rap for {currentRapperName}.";
                            _currentState.ErrorMessage = ex.Message;
                        }

                        _currentState.DebateTranscript.AppendLine($"{currentRapperName} (Turn {_currentState.CurrentTurn}):\n{_currentState.CurrentTurnText}\n");

                        // Generate speech for the current turn
                        try
                        {
                            string voice = _currentState.IsRapper1Turn ? Rapper1Voice : Rapper2Voice;
                            var newAudio = await ttsService.GenerateSpeechAsync(_currentState.CurrentTurnText, voice, cancellationToken);
                            _logger.LogInformation("🎵 Generated audio for turn {Turn}, size: {Size} bytes", _currentState.CurrentTurn, newAudio?.Length ?? 0);

                            // Log first 50 bytes for debugging
                            if (newAudio != null && newAudio.Length > 0)
                            {
                                var first50 = string.Join(" ", newAudio.Take(50).Select(b => b.ToString("X2")));
                                _logger.LogInformation("🔍 First 50 bytes of audio: {Bytes}", first50);
                            }

                            _currentState.CurrentTurnAudio = newAudio ?? Array.Empty<byte>(); // Set the new audio after generation
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error generating speech for turn {Turn}.", _currentState.CurrentTurn);
                            _currentState.ErrorMessage = ex.Message;
                            _currentState.CurrentTurnAudio = Array.Empty<byte>(); // Ensure no audio is played if generation fails
                        }

                        await NotifyStateChangeAsync();

                        // Wait for audio playback to complete before proceeding to the next turn
                        await _audioPlaybackCompletionSource.Task;
                        _audioPlaybackCompletionSource = new TaskCompletionSource<bool>(); // Reset for next turn

                        _currentState.IsRapper1Turn = !_currentState.IsRapper1Turn; // Switch turns
                    }
                }

                if (!cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Debate finished. Judging results.");
                    _currentState.IsDebateInProgress = false;
                    _currentState.IsGeneratingTurn = true; // Still busy judging
                    await NotifyStateChangeAsync();

                    // Judge the debate
                    try
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var openAiService = scope.ServiceProvider.GetRequiredService<IAzureOpenAIService>();
                            var rapperRepository = scope.ServiceProvider.GetRequiredService<IRapperRepository>();

                            var judgeResponse = await openAiService.JudgeDebateAsync(
                                _currentState.DebateTranscript.ToString(),
                                _currentState.Rapper1.Name,
                                _currentState.Rapper2.Name,
                                _currentState.Topic.Title,
                                cancellationToken);

                            _currentState.WinnerName = judgeResponse.WinnerName;
                            _currentState.JudgeReasoning = judgeResponse.Reasoning;
                            _currentState.Stats = judgeResponse.Stats;

                            if (!string.IsNullOrEmpty(_currentState.WinnerName) && _currentState.WinnerName != "Error Judging")
                            {
                                string loserName = _currentState.WinnerName == _currentState.Rapper1.Name ? _currentState.Rapper2.Name : _currentState.Rapper1.Name;
                                await rapperRepository.UpdateWinLossRecordAsync(_currentState.WinnerName, loserName);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error judging debate.");
                        _currentState.WinnerName = "Error Judging";
                        _currentState.JudgeReasoning = $"Error during judging: {ex.Message}";
                        _currentState.ErrorMessage = ex.Message;
                    }

                    _currentState.IsDebateFinished = true;
                    _currentState.IsGeneratingTurn = false; // Judging complete
                    await NotifyStateChangeAsync();
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Debate orchestration cancelled.");
                _currentState.ErrorMessage = "Debate cancelled by user.";
                _currentState.IsDebateInProgress = false;
                _currentState.IsGeneratingTurn = false;
                await NotifyStateChangeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in RunDebateTurnsAsync.");
                _currentState.ErrorMessage = $"An unhandled error occurred: {ex.Message}";
                _currentState.IsDebateInProgress = false;
                _currentState.IsGeneratingTurn = false;
                await NotifyStateChangeAsync();
            }
        }

        public Task SignalAudioPlaybackCompleteAsync()
        {
            _logger.LogDebug("SignalAudioPlaybackCompleteAsync received.");
            _audioPlaybackCompletionSource.TrySetResult(true);
            return Task.CompletedTask;
        }

        private async Task GenerateAndPlaySpeechAsync(string text, string voiceName)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var ttsService = scope.ServiceProvider.GetRequiredService<ITextToSpeechService>();
                    _currentState.CurrentTurnAudio = await ttsService.GenerateSpeechAsync(text, voiceName, CancellationToken.None);
                }
                await NotifyStateChangeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating and playing speech for intro text.");
                _currentState.ErrorMessage = ex.Message;
                _currentState.CurrentTurnAudio = Array.Empty<byte>();
                await NotifyStateChangeAsync();
            }
        }

        private async Task NotifyStateChangeAsync()
        {
            try
            {
                if (OnStateChangeAsync != null)
                {
                    await OnStateChangeAsync.Invoke(_currentState);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying state change.");
            }
        }
    }
}
