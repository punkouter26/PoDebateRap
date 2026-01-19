using Microsoft.AspNetCore.Mvc;
using PoDebateRap.Web.Services.Data;
using PoDebateRap.Shared.Models;

namespace PoDebateRap.Web.Endpoints;

/// <summary>
/// Minimal API endpoints for managing rapper profiles and records.
/// </summary>
public static class RapperEndpoints
{
    public static void MapRapperEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/rappers")
            .WithTags("Rappers")
            .WithOpenApi();

        group.MapGet("/", GetAllRappers)
            .WithName("GetAllRappers")
            .WithSummary("Retrieves all available rappers.")
            .Produces<IEnumerable<Rapper>>();

        group.MapPatch("/{id}/record", UpdateRapperRecord)
            .WithName("UpdateRapperRecord")
            .WithSummary("Updates the win/loss record for a specific rapper after a battle.")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetAllRappers(IRapperRepository rapperRepository)
    {
        var rappers = await rapperRepository.GetAllRappersAsync();
        return Results.Ok(rappers);
    }

    private static async Task<IResult> UpdateRapperRecord(
        string id,
        [FromBody] UpdateRecordRequest request,
        IRapperRepository rapperRepository)
    {
        await rapperRepository.UpdateWinLossRecordAsync(request.WinnerName, request.LoserName);
        return Results.Ok();
    }
}

/// <summary>
/// Request model for updating rapper win/loss records.
/// </summary>
public record UpdateRecordRequest(string WinnerName, string LoserName);
