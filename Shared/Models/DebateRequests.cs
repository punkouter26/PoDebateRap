using System.Text; // For StringBuilder

namespace PoDebateRap.Shared.Models
{
    public class StartDebateRequest
    {
        public Rapper Rapper1 { get; set; }
        public Rapper Rapper2 { get; set; }
        public Topic Topic { get; set; }
    }

    public class DebateState
    {
        public Rapper Rapper1 { get; set; }
        public Rapper Rapper2 { get; set; }
        public Topic Topic { get; set; }
        public bool IsDebateInProgress { get; set; }
        public bool IsDebateFinished { get; set; }
        public int CurrentTurn { get; set; }
        public int CurrentTurnNumber => CurrentTurn; // Alias for compatibility
        public int TotalTurns { get; set; }
        public StringBuilder DebateTranscript { get; set; } = new StringBuilder();
        public string CurrentTurnText { get; set; } = string.Empty;
        public bool IsRapper1Turn { get; set; }
        public byte[] CurrentTurnAudio { get; set; } // Base64 encoded audio
        public string WinnerName { get; set; }
        public string JudgeReasoning { get; set; }
        public DebateStats Stats { get; set; }
        public string ErrorMessage { get; set; }
        public bool IsGeneratingTurn { get; set; }
    }
}
