using Microsoft.AspNetCore.SignalR;
using PoDebateRap.Web.Services.Orchestration;
using System.Threading.Tasks;

namespace PoDebateRap.Web.Hubs
{
    public class DebateHub : Hub
    {
        private readonly IDebateOrchestrator _orchestrator;

        public DebateHub(IDebateOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        public async Task SignalAudioPlaybackComplete()
        {
            await _orchestrator.SignalAudioPlaybackCompleteAsync();
        }
    }
}
