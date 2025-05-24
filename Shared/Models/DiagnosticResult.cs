namespace PoDebateRap.Shared.Models
{
    public class DiagnosticResult
    {
        public string CheckName { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
