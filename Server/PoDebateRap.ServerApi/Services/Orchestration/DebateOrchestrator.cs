using Microsoft.Extensions.Logging;
using PoDebateRap.Shared.Models;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PoDebateRap.ServerApi.Services.AI;
using PoDebateRap.ServerApi.Services.Speech;
using PoDebateRap.ServerApi.Services.Data;
using PoDebateRap.ServerApi.Services.Factories;

namespace PoDebateRap.ServerApi.Services.Orchestration
{
    public class DebateOrchestrator : IDebateOrchestrator
    {
        private readonly ILogger<DebateOrchestrator> _logger;
        private readonly IDebateServiceFactory _serviceFactory;

        private DebateState _currentState;
        public DebateState CurrentState => _currentState;

        public event Func<DebateState, Task> OnStateChangeAsync = null!;

        private CancellationTokenSource? _debateCancellationTokenSource;
        private TaskCompletionSource<bool> _audioPlaybackCompletionSource;

        private const int MaxDebateTurns = 10; // Total turns for the debate
        private const int MaxTokensPerTurn = 200; // Max tokens for AI generated rap
        private const string Rapper1Voice = "en-US-GuyNeural"; // Example voice for Rapper 1
        private const string Rapper2Voice = "en-US-JennyNeural"; // Example voice for Rapper 2

        public DebateOrchestrator(ILogger<DebateOrchestrator> logger, IDebateServiceFactory serviceFactory)
        {
            _logger = logger;
            _serviceFactory = serviceFactory;
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

            InitializeDebateState(rapper1, rapper2, topic);
            await NotifyStateChangeAsync();

            await GenerateIntroductionAsync();

            _logger.LogInformation("Debate started: {R1} vs {R2} on {Topic}", rapper1.Name, rapper2.Name, topic.Title);

            // Start the debate turns in a background task
            _ = Task.Run(() => RunDebateTurnsAsync(_debateCancellationTokenSource.Token));
        }

        private void InitializeDebateState(Rapper rapper1, Rapper rapper2, Topic topic)
        {
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
                IsGeneratingTurn = false,
                CurrentTurnAudio = Array.Empty<byte>(),
                WinnerName = string.Empty,
                JudgeReasoning = string.Empty,
                Stats = new DebateStats(),
                ErrorMessage = string.Empty
            };
        }

        private async Task GenerateIntroductionAsync()
        {
            // Initial audio for the intro text
            await GenerateAndPlaySpeechAsync(_currentState.CurrentTurnText, Rapper1Voice);
            // Do NOT wait for audio playback to complete here. The client will play the audio
            // and signal completion, allowing the API call to return immediately.
            _audioPlaybackCompletionSource = new TaskCompletionSource<bool>(); // Reset for next turn
        }

        private async Task RunDebateTurnsAsync(CancellationToken cancellationToken)
        {
            try
            {
                await ExecuteDebateLoopAsync(cancellationToken);

                if (!cancellationToken.IsCancellationRequested)
                {
                    await FinalizeDebateAsync(cancellationToken);
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

        private async Task ExecuteDebateLoopAsync(CancellationToken cancellationToken)
        {
            while (_currentState.CurrentTurn < MaxDebateTurns && !cancellationToken.IsCancellationRequested)
            {
                using (var serviceScope = _serviceFactory.CreateScope())
                {
                    await ExecuteSingleTurnAsync(serviceScope, cancellationToken);
                }

                // Wait for audio playback to complete before proceeding to the next turn
                await _audioPlaybackCompletionSource.Task;
                _audioPlaybackCompletionSource = new TaskCompletionSource<bool>(); // Reset for next turn

                _currentState.IsRapper1Turn = !_currentState.IsRapper1Turn; // Switch turns
            }
        }

        private async Task ExecuteSingleTurnAsync(IDebateServiceScope serviceScope, CancellationToken cancellationToken)
        {
            _currentState.CurrentTurn++;
            _currentState.IsGeneratingTurn = true;
            _currentState.ErrorMessage = string.Empty;
            await NotifyStateChangeAsync();

            var (currentRapperName, opponentRapperName, role) = GetCurrentTurnInfo();

            _logger.LogInformation("Generating turn {Turn} for {Rapper} ({Role}).", _currentState.CurrentTurn, currentRapperName, role);

            await GenerateTurnTextAsync(serviceScope.AIService, currentRapperName, opponentRapperName, role, cancellationToken);
            await GenerateTurnAudioAsync(serviceScope.TTSService, currentRapperName, cancellationToken);

            await NotifyStateChangeAsync();
        }

        private (string currentRapper, string opponent, string role) GetCurrentTurnInfo()
        {
            string currentRapper = _currentState.IsRapper1Turn ? _currentState.Rapper1.Name : _currentState.Rapper2.Name;
            string opponent = _currentState.IsRapper1Turn ? _currentState.Rapper2.Name : _currentState.Rapper1.Name;
            string role = _currentState.IsRapper1Turn ? "Pro" : "Con";
            return (currentRapper, opponent, role);
        }

        private async Task GenerateTurnTextAsync(IAzureOpenAIService aiService, string currentRapper, string opponent, string role, CancellationToken cancellationToken)
        {
            string prompt = $"You are {currentRapper} debating {opponent} on the topic '{_currentState.Topic.Title}'. " +
                            $"Your role is {role}. This is turn {_currentState.CurrentTurn} of {MaxDebateTurns}. " +
                            $"Current transcript:\n{_currentState.DebateTranscript.ToString()}\n" +
                            $"Your rap:";

            try
            {
                _currentState.CurrentTurnText = await aiService.GenerateDebateTurnAsync(prompt, MaxTokensPerTurn, cancellationToken);
                _currentState.DebateTranscript.AppendLine($"{currentRapper} (Turn {_currentState.CurrentTurn}):\n{_currentState.CurrentTurnText}\n");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating turn {Turn} text.", _currentState.CurrentTurn);
                _currentState.CurrentTurnText = $"Error generating rap for {currentRapper}.";
                _currentState.ErrorMessage = ex.Message;
            }
        }

        private async Task GenerateTurnAudioAsync(ITextToSpeechService ttsService, string currentRapper, CancellationToken cancellationToken)
        {
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

                _currentState.CurrentTurnAudio = newAudio ?? Array.Empty<byte>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating speech for turn {Turn}.", _currentState.CurrentTurn);
                _currentState.ErrorMessage = ex.Message;
                _currentState.CurrentTurnAudio = Array.Empty<byte>();
            }
        }

        private async Task FinalizeDebateAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Debate finished. Judging results.");
            _currentState.IsDebateInProgress = false;
            _currentState.IsGeneratingTurn = true; // Still busy judging
            await NotifyStateChangeAsync();

            await JudgeDebateAsync(cancellationToken);

            _currentState.IsDebateFinished = true;
            _currentState.IsGeneratingTurn = false;
            await NotifyStateChangeAsync();
        }

        private async Task JudgeDebateAsync(CancellationToken cancellationToken)
        {
            try
            {
                using (var serviceScope = _serviceFactory.CreateScope())
                {
                    var judgeResponse = await serviceScope.AIService.JudgeDebateAsync(
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
                        await UpdateWinLossRecordsAsync(serviceScope.RapperRepository);
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
        }

        private async Task UpdateWinLossRecordsAsync(IRapperRepository rapperRepository)
        {
            string loserName = _currentState.WinnerName == _currentState.Rapper1.Name
                ? _currentState.Rapper2.Name
                : _currentState.Rapper1.Name;

            await rapperRepository.UpdateWinLossRecordAsync(_currentState.WinnerName, loserName);
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
                using (var serviceScope = _serviceFactory.CreateScope())
                {
                    var ttsService = serviceScope.TTSService;
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
