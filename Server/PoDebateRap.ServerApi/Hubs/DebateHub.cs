using Microsoft.AspNetCore.SignalR;
using PoDebateRap.ServerApi.Services.Orchestration;
using System.Threading.Tasks;

namespace PoDebateRap.ServerApi.Hubs
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
