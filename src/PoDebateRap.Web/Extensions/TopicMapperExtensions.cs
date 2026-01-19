using PoDebateRap.Shared.Models;

namespace PoDebateRap.Web.Extensions;

/// <summary>
/// Extension methods for mapping Topic objects.
/// Eliminates duplicate Topic conversion logic across controllers.
/// </summary>
public static class TopicMapperExtensions
{
    /// <summary>
    /// Converts a collection of NewsHeadline to Topic objects.
    /// </summary>
    public static IEnumerable<Topic> ToTopics(this IEnumerable<NewsHeadline> headlines, string category = "Current Events")
    {
        return headlines.Select(h => h.ToTopic(category));
    }

    /// <summary>
    /// Converts a NewsHeadline to a Topic object.
    /// </summary>
    public static Topic ToTopic(this NewsHeadline headline, string category = "Current Events")
    {
        return new Topic
        {
            Title = headline.Title?.Trim() ?? string.Empty,
            Category = category
        };
    }

    /// <summary>
    /// Gets the first headline as a topic or returns null.
    /// </summary>
    public static Topic? ToLatestTopic(this IEnumerable<NewsHeadline> headlines, string category = "Breaking News")
    {
        var first = headlines.FirstOrDefault();
        return first?.ToTopic(category);
    }
}
