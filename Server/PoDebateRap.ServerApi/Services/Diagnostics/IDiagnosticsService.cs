using PoDebateRap.Shared.Models;

namespace PoDebateRap.ServerApi.Services.Diagnostics
{
    public interface IDiagnosticsService
    {
        Task<List<DiagnosticResult>> RunAllChecksAsync();
    }
}
