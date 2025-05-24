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
            _newsApiKey = _configuration["NewsApi:ApiKey"] ?? throw new ArgumentNullException("NewsApi:ApiKey not found in configuration.");
            _httpClient.BaseAddress = new Uri("https://newsapi.org/v2/");
        }

        public async Task<List<NewsHeadline>> GetTopHeadlinesAsync(int count)
        {
            try
            {
                _logger.LogInformation("Fetching top {Count} news headlines.", count);
                var response = await _httpClient.GetFromJsonAsync<NewsApiResponse>($"top-headlines?country=us&apiKey={_newsApiKey}");

                if (response?.Articles == null || !response.Articles.Any())
                {
                    _logger.LogWarning("News API returned no articles or null response. Falling back to hardcoded topic.");
                    return new List<NewsHeadline> { new NewsHeadline { Title = "End of the world is coming", Url = "https://example.com/fallback" } };
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
                return new List<NewsHeadline> { new NewsHeadline { Title = "End of the world is coming", Url = "https://example.com/fallback" } };
            }
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
