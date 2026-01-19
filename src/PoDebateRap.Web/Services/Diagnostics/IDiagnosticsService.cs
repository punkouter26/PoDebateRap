using PoDebateRap.Shared.Models;

namespace PoDebateRap.Web.Services.Diagnostics
{
    public interface IDiagnosticsService
    {
        Task<List<DiagnosticResult>> RunAllChecksAsync();
    }
}
