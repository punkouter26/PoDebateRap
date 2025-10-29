using PoDebateRap.Shared.Models;
using System.Threading;
using System.Threading.Tasks;

namespace PoDebateRap.ServerApi.Services.AI
{
    public interface IAzureOpenAIService
    {
        Task<string> GenerateDebateTurnAsync(string prompt, int maxTokens, CancellationToken cancellationToken);
        Task<JudgeDebateResponse> JudgeDebateAsync(string debateTranscript, string rapper1Name, string rapper2Name, string topic, CancellationToken cancellationToken);
    }
}
