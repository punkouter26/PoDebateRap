using PoDebateRap.Web.Services.Diagnostics;
using PoDebateRap.Shared.Models;

namespace PoDebateRap.Web.Endpoints;

/// <summary>
/// Minimal API endpoints for running system diagnostics and health checks.
/// </summary>
public static class DiagnosticsEndpoints
{
    public static void MapDiagnosticsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/diagnostics")
            .WithTags("Diagnostics")
            .WithOpenApi();

        group.MapGet("/", GetAllDiagnostics)
            .WithName("GetAllDiagnostics")
            .WithSummary("Runs all diagnostic checks and returns detailed results.")
            .Produces<List<DiagnosticResult>>();
    }

    private static async Task<IResult> GetAllDiagnostics(IDiagnosticsService diagnosticsService)
    {
        var results = await diagnosticsService.RunAllChecksAsync();
        return Results.Ok(results);
    }
}
