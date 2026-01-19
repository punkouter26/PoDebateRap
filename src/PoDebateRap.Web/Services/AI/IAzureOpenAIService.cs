using PoDebateRap.Shared.Models;
using System.Threading;
using System.Threading.Tasks;

namespace PoDebateRap.Web.Services.AI
{
    public interface IAzureOpenAIService
    {
        /// <summary>
        /// Indicates whether the Azure OpenAI service is properly configured.
        /// </summary>
        bool IsConfigured { get; }

        Task<string> GenerateDebateTurnAsync(string prompt, int maxTokens, CancellationToken cancellationToken);
        Task<JudgeDebateResponse> JudgeDebateAsync(string debateTranscript, string rapper1Name, string rapper2Name, string topic, CancellationToken cancellationToken);
    }
}
