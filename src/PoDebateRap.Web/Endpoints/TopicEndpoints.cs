using PoDebateRap.Web.Services.News;
using PoDebateRap.Web.Extensions;
using PoDebateRap.Shared.Models;

namespace PoDebateRap.Web.Endpoints;

/// <summary>
/// Minimal API endpoints for managing debate topics derived from news headlines.
/// </summary>
public static class TopicEndpoints
{
    public static void MapTopicEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/topics")
            .WithTags("Topics")
            .WithOpenApi();

        group.MapGet("/", GetAllTopics)
            .WithName("GetAllTopics")
            .WithSummary("Retrieves a list of available debate topics from current news headlines.")
            .Produces<IEnumerable<Topic>>();

        group.MapGet("/latest", GetLatestTopic)
            .WithName("GetLatestTopic")
            .WithSummary("Retrieves the latest breaking news topic for a debate.")
            .Produces<Topic>()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetAllTopics(INewsService newsService, ILogger<Program> logger)
    {
        var headlines = await newsService.GetTopHeadlinesAsync(10);
        var topics = headlines.ToTopics().ToList();
        logger.LogInformation("Retrieved {Count} topics from latest news headlines.", topics.Count);
        return Results.Ok(topics);
    }

    private static async Task<IResult> GetLatestTopic(INewsService newsService, ILogger<Program> logger)
    {
        var headlines = await newsService.GetTopHeadlinesAsync(1);
        var latestTopic = headlines.ToLatestTopic();
        
        if (latestTopic is not null)
        {
            logger.LogInformation("Retrieved latest topic: {Title}", latestTopic.Title);
            return Results.Ok(latestTopic);
        }
        
        logger.LogWarning("No news headlines available for topic generation.");
        return Results.NotFound("No current news topics available");
    }
}
