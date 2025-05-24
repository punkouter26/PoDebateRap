using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using PoDebateRap.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PoDebateRap.ServerApi.Services.Data
{
    public class TopicRepository : ITopicRepository
    {
        private const string TableName = "Topics";
        private readonly ITableStorageService _tableStorageService;
        private readonly ILogger<TopicRepository> _logger;

        public TopicRepository(ITableStorageService tableStorageService, ILogger<TopicRepository> logger)
        {
            _tableStorageService = tableStorageService;
            _logger = logger;
        }

        public async Task<List<Topic>> GetAllTopicsAsync()
        {
            try
            {
                var topics = new List<Topic>();
                await foreach (var entity in _tableStorageService.GetEntitiesAsync<TopicEntity>(TableName))
                {
                    topics.Add(new Topic
                    {
                        Title = entity.RowKey,
                        Category = entity.Category
                    });
                }
                return topics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all topics from Table Storage.");
                throw;
            }
        }

        public async Task SeedInitialTopicsAsync()
        {
            _logger.LogInformation("Checking if initial topics need to be seeded...");
            var existingTopics = await GetAllTopicsAsync();
            if (!existingTopics.Any())
            {
                _logger.LogInformation("No topics found. Seeding initial data.");
                var initialTopics = new List<TopicEntity>
                {
                    new TopicEntity("Politics", "The future of AI in governance"),
                    new TopicEntity("Technology", "The ethics of self-driving cars"),
                    new TopicEntity("Science", "Colonizing Mars: necessity or folly?"),
                    new TopicEntity("Culture", "The impact of social media on mental health"),
                    new TopicEntity("Economy", "Universal Basic Income: solution or fantasy?")
                };

                foreach (var topic in initialTopics)
                {
                    await _tableStorageService.UpsertEntityAsync(TableName, topic);
                }
                _logger.LogInformation("Initial topics seeded successfully.");
            }
            else
            {
                _logger.LogInformation("Topics already exist. Skipping seeding.");
            }
        }

        // Table Entity for Topic
        public class TopicEntity : ITableEntity
        {
            public string PartitionKey { get; set; }
            public string RowKey { get; set; } // Topic Title
            public string Category { get; set; }
            public DateTimeOffset? Timestamp { get; set; }
            public Azure.ETag ETag { get; set; }

            public TopicEntity() { } // Parameterless constructor for deserialization

            public TopicEntity(string category, string title)
            {
                PartitionKey = category;
                RowKey = title;
                Category = category;
            }
        }
    }
}
