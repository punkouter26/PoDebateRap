using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PoDebateRap.ServerApi.Services.Data;
using Azure.Data.Tables;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PoDebateRap.IntegrationTests
{
    public class TableStorageServiceIntegrationTests
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TableStorageService> _logger;
        private readonly string _testTableName = "TestTableStorage";

        public TableStorageServiceIntegrationTests()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json", optional: true)
                .AddUserSecrets<TableStorageServiceIntegrationTests>()
                .AddEnvironmentVariables()
                .Build();

            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<TableStorageService>();
        }

        // A simple test entity for Table Storage
        public class TestEntity : ITableEntity
        {
            public string PartitionKey { get; set; }
            public string RowKey { get; set; }
            public string Message { get; set; }
            public DateTimeOffset? Timestamp { get; set; }
            public Azure.ETag ETag { get; set; }

            public TestEntity() { }

            public TestEntity(string partitionKey, string rowKey, string message)
            {
                PartitionKey = partitionKey;
                RowKey = rowKey;
                Message = message;
            }
        }

        [Fact(Skip = "Requires a valid Azure Storage Connection String and active storage account. Run manually or with specific CI setup.")]
        public async Task UpsertAndGetEntityAsync_WorksCorrectly()
        {
            // Arrange
            var service = new TableStorageService(_configuration, _logger);
            var entity = new TestEntity("TestPartition", "TestRow1", "Hello Table Storage!");

            // Act - Upsert
            await service.UpsertEntityAsync(_testTableName, entity);

            // Assert - Get
            var retrievedEntity = await service.GetEntityAsync<TestEntity>(_testTableName, entity.PartitionKey, entity.RowKey);

            Assert.NotNull(retrievedEntity);
            Assert.Equal(entity.PartitionKey, retrievedEntity.PartitionKey);
            Assert.Equal(entity.RowKey, retrievedEntity.RowKey);
            Assert.Equal(entity.Message, retrievedEntity.Message);

            // Clean up
            await service.DeleteEntityAsync<TestEntity>(_testTableName, entity.PartitionKey, entity.RowKey);
        }

        [Fact(Skip = "Requires a valid Azure Storage Connection String and active storage account. Run manually or with specific CI setup.")]
        public async Task GetEntitiesAsync_ReturnsAllEntitiesInPartition()
        {
            // Arrange
            var service = new TableStorageService(_configuration, _logger);
            var partitionKey = "TestPartitionForGetAll";
            var entities = new List<TestEntity>
            {
                new TestEntity(partitionKey, "Row1", "Data1"),
                new TestEntity(partitionKey, "Row2", "Data2"),
                new TestEntity(partitionKey, "Row3", "Data3")
            };

            foreach (var entity in entities)
            {
                await service.UpsertEntityAsync(_testTableName, entity);
            }

            // Act
            var retrievedEntities = new List<TestEntity>();
            await foreach (var entity in service.GetEntitiesAsync<TestEntity>(_testTableName, $"PartitionKey eq '{partitionKey}'"))
            {
                retrievedEntities.Add(entity);
            }

            // Assert
            Assert.Equal(entities.Count, retrievedEntities.Count);
            Assert.Contains(retrievedEntities, e => e.RowKey == "Row1" && e.Message == "Data1");
            Assert.Contains(retrievedEntities, e => e.RowKey == "Row2" && e.Message == "Data2");
            Assert.Contains(retrievedEntities, e => e.RowKey == "Row3" && e.Message == "Data3");

            // Clean up
            foreach (var entity in entities)
            {
                await service.DeleteEntityAsync<TestEntity>(_testTableName, entity.PartitionKey, entity.RowKey);
            }
        }

        [Fact(Skip = "Requires a valid Azure Storage Connection String and active storage account. Run manually or with specific CI setup.")]
        public async Task DeleteEntityAsync_RemovesEntity()
        {
            // Arrange
            var service = new TableStorageService(_configuration, _logger);
            var entity = new TestEntity("TestPartitionForDelete", "TestRowToDelete", "To be deleted");
            await service.UpsertEntityAsync(_testTableName, entity);

            // Act
            await service.DeleteEntityAsync<TestEntity>(_testTableName, entity.PartitionKey, entity.RowKey);

            // Assert
            var retrievedEntity = await service.GetEntityAsync<TestEntity>(_testTableName, entity.PartitionKey, entity.RowKey);
            Assert.Null(retrievedEntity);
        }
    }
}
