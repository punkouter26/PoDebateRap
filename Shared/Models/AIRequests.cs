using PoDebateRap.Shared.Models; // For DebateStats

namespace PoDebateRap.Shared.Models
{
    public class GenerateDebateTurnRequest
    {
        public string Prompt { get; set; }
        public int MaxTokens { get; set; }
    }

    public class JudgeDebateRequest
    {
        public string DebateTranscript { get; set; }
        public string Rapper1Name { get; set; }
        public string Rapper2Name { get; set; }
        public string Topic { get; set; }
    }

    public class JudgeDebateResponse
    {
        public string WinnerName { get; set; }
        public string Reasoning { get; set; }
        public DebateStats Stats { get; set; }
    }

    public class GenerateSpeechRequest
    {
        public string Text { get; set; }
        public string VoiceName { get; set; }
    }
}
