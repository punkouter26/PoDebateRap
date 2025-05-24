using PoDebateRap.Shared.Models;
using System.Threading.Tasks;

namespace PoDebateRap.ServerApi.Services.Orchestration
{
    public interface IDebateOrchestrator
    {
        DebateState CurrentState { get; }
        event Func<DebateState, Task> OnStateChangeAsync;
        Task StartNewDebateAsync(Rapper rapper1, Rapper rapper2, Topic topic);
        Task SignalAudioPlaybackCompleteAsync();
        void ResetDebate();
    }
}
