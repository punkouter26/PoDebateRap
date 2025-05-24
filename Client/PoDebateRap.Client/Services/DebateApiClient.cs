using PoDebateRap.Shared.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;

namespace PoDebateRap.Client.Services
{
    public class DebateApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DebateApiClient> _logger;

        public DebateApiClient(HttpClient httpClient, ILogger<DebateApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<DebateState> StartNewDebateAsync(Rapper rapper1, Rapper rapper2, Topic topic)
        {
            try
            {
                var request = new StartDebateRequest { Rapper1 = rapper1, Rapper2 = rapper2, Topic = topic };
                var response = await _httpClient.PostAsJsonAsync("Debate/start", request);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<DebateState>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting new debate via API.");
                throw;
            }
        }

        public async Task<DebateState?> GetCurrentDebateStateAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<DebateState>("Debate/state");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current debate state via API.");
                throw;
            }
        }

        public async Task SignalAudioPlaybackCompleteAsync()
        {
            try
            {
                var response = await _httpClient.PostAsync("Debate/signal-audio-complete", null);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error signaling audio playback complete via API.");
                throw;
            }
        }

        public async Task ResetDebateAsync()
        {
            try
            {
                var response = await _httpClient.PostAsync("Debate/reset", null);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting debate via API.");
                throw;
            }
        }
    }

}
