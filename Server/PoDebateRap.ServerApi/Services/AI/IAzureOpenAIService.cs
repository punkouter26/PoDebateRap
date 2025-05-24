using PoDebateRap.Shared.Models;
using System.Threading.Tasks;

namespace PoDebateRap.ServerApi.Services.AI
{
    public interface IAzureOpenAIService
    {
        Task<string> GenerateDebateTurnAsync(string prompt, int maxTokens);
        Task<JudgeDebateResponse> JudgeDebateAsync(string debateTranscript, string rapper1Name, string rapper2Name, string topic);
        Task<byte[]> GenerateSpeechAsync(string text, string voiceName);
    }
}
