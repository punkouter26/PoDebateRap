using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PoDebateRap.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PoDebateRap.ServerApi.Services.News
{
    public class NewsService : INewsService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NewsService> _logger;
        private readonly string _newsApiKey;

        public NewsService(HttpClient httpClient, IConfiguration configuration, ILogger<NewsService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _newsApiKey = _configuration["NewsApi:ApiKey"] ?? "";

            _logger.LogWarning("NewsAPI Key value: '{ApiKey}' (Length: {Length})", _newsApiKey, _newsApiKey.Length);

            if (string.IsNullOrWhiteSpace(_newsApiKey))
            {
                _logger.LogWarning("NewsApi:ApiKey not found in configuration. Will use fallback topics.");
            }
            _httpClient.BaseAddress = new Uri("https://newsapi.org/v2/");
        }

        public async Task<List<NewsHeadline>> GetTopHeadlinesAsync(int count)
        {
            try
            {
                // If no API key is configured, return fallback topics immediately
                if (string.IsNullOrWhiteSpace(_newsApiKey))
                {
                    _logger.LogInformation("No NewsAPI key configured, using fallback topics.");
                    return GetFallbackTopics(count);
                }

                _logger.LogInformation("Fetching top {Count} news headlines.", count);
                var response = await _httpClient.GetFromJsonAsync<NewsApiResponse>($"top-headlines?country=us&apiKey={_newsApiKey}");

                if (response?.Articles == null || !response.Articles.Any())
                {
                    _logger.LogWarning("News API returned no articles or null response. Using fallback topics.");
                    return GetFallbackTopics(count);
                }

                var headlines = response.Articles
                    .Where(a => !string.IsNullOrWhiteSpace(a.Title) && !string.IsNullOrWhiteSpace(a.Url))
                    .Take(count)
                    .Select(a => new NewsHeadline { Title = a.Title, Url = a.Url })
                    .ToList();

                _logger.LogInformation("Fetched {FetchedCount} headlines.", headlines.Count);
                return headlines;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching news headlines from NewsAPI. Falling back to hardcoded topic.");
                return GetFallbackTopics(count);
            }
        }

        private List<NewsHeadline> GetFallbackTopics(int count)
        {
            var fallbackTopics = new List<NewsHeadline>
            {
                new NewsHeadline { Title = "Artificial Intelligence vs Human Creativity", Url = "https://example.com/ai-creativity" },
                new NewsHeadline { Title = "Social Media Impact on Society", Url = "https://example.com/social-media" },
                new NewsHeadline { Title = "Climate Change Solutions", Url = "https://example.com/climate" },
                new NewsHeadline { Title = "Future of Remote Work", Url = "https://example.com/remote-work" },
                new NewsHeadline { Title = "Electric Cars vs Gas Cars", Url = "https://example.com/electric-cars" },
                new NewsHeadline { Title = "Space Exploration Priorities", Url = "https://example.com/space" },
                new NewsHeadline { Title = "Healthy Lifestyle Choices", Url = "https://example.com/health" },
                new NewsHeadline { Title = "Education System Reform", Url = "https://example.com/education" }
            };

            var random = new Random();
            return fallbackTopics.OrderBy(x => random.Next()).Take(count).ToList();
        }

        private class NewsApiResponse
        {
            public string Status { get; set; }
            public int TotalResults { get; set; }
            public List<Article> Articles { get; set; }
        }

        private class Article
        {
            public string Title { get; set; }
            public string Url { get; set; }
            // Add other properties if needed, e.g., Description, Author, Source
        }
    }
}
