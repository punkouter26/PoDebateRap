using Microsoft.AspNetCore.Mvc;
using PoDebateRap.Web.Services.Orchestration;
using PoDebateRap.Shared.Models;

namespace PoDebateRap.Web.Endpoints;

/// <summary>
/// Minimal API endpoints for managing rap debate sessions.
/// </summary>
public static class DebateEndpoints
{
    public static void MapDebateEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/debate")
            .WithTags("Debate")
            .WithOpenApi();

        group.MapPost("/", CreateDebate)
            .WithName("CreateDebate")
            .WithSummary("Initiates a new rap debate between two rappers.")
            .Produces<DebateState>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/current", GetCurrentDebate)
            .WithName("GetCurrentDebate")
            .WithSummary("Retrieves the current state of the active debate.")
            .Produces<DebateState>();

        group.MapPatch("/current/audio-status", UpdateAudioStatus)
            .WithName("UpdateAudioStatus")
            .WithSummary("Updates the audio playback status to signal completion.");

        group.MapDelete("/current", DeleteCurrentDebate)
            .WithName("DeleteCurrentDebate")
            .WithSummary("Cancels and resets the current debate session.")
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> CreateDebate(
        [FromBody] StartDebateRequest request,
        IDebateOrchestrator orchestrator,
        ILogger<Program> logger)
    {
        await orchestrator.StartNewDebateAsync(request.Rapper1, request.Rapper2, request.Topic);
        return Results.Ok(orchestrator.CurrentState);
    }

    private static IResult GetCurrentDebate(
        IDebateOrchestrator orchestrator,
        ILogger<Program> logger)
    {
        logger.LogInformation("GetCurrentDebate: Returning state. IsDebateInProgress: {InProg}, IsGeneratingTurn: {Gen}",
            orchestrator.CurrentState.IsDebateInProgress, orchestrator.CurrentState.IsGeneratingTurn);
        return Results.Ok(orchestrator.CurrentState);
    }

    private static async Task<IResult> UpdateAudioStatus(
        IDebateOrchestrator orchestrator)
    {
        await orchestrator.SignalAudioPlaybackCompleteAsync();
        return Results.Ok();
    }

    private static IResult DeleteCurrentDebate(
        IDebateOrchestrator orchestrator)
    {
        orchestrator.ResetDebate();
        return Results.NoContent();
    }
}
