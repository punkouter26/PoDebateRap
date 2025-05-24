using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PoDebateRap.ServerApi.Services.Data
{
    public class TableStorageService : ITableStorageService
    {
        private readonly string _connectionString;
        private readonly ILogger<TableStorageService> _logger;

        public TableStorageService(IConfiguration configuration, ILogger<TableStorageService> logger)
        {
            _connectionString = configuration["Azure:StorageConnectionString"] ?? throw new ArgumentNullException("Azure:StorageConnectionString not found in configuration.");
            _logger = logger;
        }

        private TableServiceClient GetTableServiceClient()
        {
            return new TableServiceClient(_connectionString);
        }

        public async Task<TableClient> GetTableClientAsync(string tableName)
        {
            var serviceClient = GetTableServiceClient();
            var tableClient = serviceClient.GetTableClient(tableName);
            await tableClient.CreateIfNotExistsAsync();
            return tableClient;
        }

        public async Task<T> GetEntityAsync<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity
        {
            try
            {
                var tableClient = await GetTableClientAsync(tableName);
                return await tableClient.GetEntityAsync<T>(partitionKey, rowKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting entity from table {TableName} with PartitionKey {PartitionKey} and RowKey {RowKey}.", tableName, partitionKey, rowKey);
                return null;
            }
        }

        public async IAsyncEnumerable<T> GetEntitiesAsync<T>(string tableName, string filter = null) where T : class, ITableEntity
        {
            var tableClient = await GetTableClientAsync(tableName);
            var query = tableClient.QueryAsync<T>(filter);

            await foreach (var entity in query)
            {
                yield return entity;
            }
        }

        public async Task AddEntityAsync<T>(string tableName, T entity) where T : class, ITableEntity
        {
            try
            {
                var tableClient = await GetTableClientAsync(tableName);
                await tableClient.AddEntityAsync(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding entity to table {TableName}.", tableName);
                throw;
            }
        }

        public async Task UpsertEntityAsync<T>(string tableName, T entity) where T : class, ITableEntity
        {
            try
            {
                var tableClient = await GetTableClientAsync(tableName);
                await tableClient.UpsertEntityAsync(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error upserting entity to table {TableName}.", tableName);
                throw;
            }
        }

        public async Task DeleteEntityAsync<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity
        {
            try
            {
                var tableClient = await GetTableClientAsync(tableName);
                await tableClient.DeleteEntityAsync(partitionKey, rowKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting entity from table {TableName} with PartitionKey {PartitionKey} and RowKey {RowKey}.", tableName, partitionKey, rowKey);
                throw;
            }
        }
    }
}
