using PoDebateRap.Shared.Models;
using System.Threading.Tasks;

namespace PoDebateRap.Web.Services.Orchestration
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
