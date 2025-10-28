using System.Text; // For StringBuilder

namespace PoDebateRap.Shared.Models
{
    public class StartDebateRequest
    {
        public required Rapper Rapper1 { get; set; }
        public required Rapper Rapper2 { get; set; }
        public required Topic Topic { get; set; }
    }

    public class DebateState
    {
        public required Rapper Rapper1 { get; set; }
        public required Rapper Rapper2 { get; set; }
        public required Topic Topic { get; set; }
        public bool IsDebateInProgress { get; set; }
        public bool IsDebateFinished { get; set; }
        public int CurrentTurn { get; set; }
        public int CurrentTurnNumber => CurrentTurn; // Alias for compatibility
        public int TotalTurns { get; set; }
        public StringBuilder DebateTranscript { get; set; } = new StringBuilder();
        public string CurrentTurnText { get; set; } = string.Empty;
        public bool IsRapper1Turn { get; set; }
        public required byte[] CurrentTurnAudio { get; set; } // Base64 encoded audio
        public required string WinnerName { get; set; }
        public required string JudgeReasoning { get; set; }
        public required DebateStats Stats { get; set; }
        public required string ErrorMessage { get; set; }
        public bool IsGeneratingTurn { get; set; }
    }
}
