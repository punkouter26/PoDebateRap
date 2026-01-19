using PoDebateRap.Web.Services.News;
using PoDebateRap.Web.Extensions;
using PoDebateRap.Shared.Models;

namespace PoDebateRap.Web.Endpoints;

/// <summary>
/// Minimal API endpoints for accessing news headlines.
/// </summary>
public static class NewsEndpoints
{
    public static void MapNewsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/news")
            .WithTags("News")
            .WithOpenApi();

        group.MapGet("/headlines", GetHeadlines)
            .WithName("GetHeadlines")
            .WithSummary("Retrieves the latest news headlines.")
            .Produces<IEnumerable<NewsHeadline>>();

        // Legacy endpoints for backwards compatibility
        group.MapGet("/topics", GetTopics)
            .WithName("GetNewsTopics")
            .WithSummary("Gets topics from news headlines (legacy - use /api/topics instead).")
            .Produces<IEnumerable<Topic>>();

        group.MapGet("/topics/latest", GetLatestTopic)
            .WithName("GetLatestNewsTopic")
            .WithSummary("Gets the latest topic from news headlines (legacy - use /api/topics/latest instead).")
            .Produces<Topic>()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetHeadlines(INewsService newsService)
    {
        var headlines = await newsService.GetTopHeadlinesAsync(1);
        return Results.Ok(headlines);
    }

    private static async Task<IResult> GetTopics(INewsService newsService, ILogger<Program> logger)
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
