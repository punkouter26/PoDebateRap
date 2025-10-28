using Azure.Data.Tables;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PoDebateRap.ServerApi.Services.Data
{
    public interface ITableStorageService
    {
        Task<TableClient> GetTableClientAsync(string tableName);
        Task<T?> GetEntityAsync<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity;
        IAsyncEnumerable<T> GetEntitiesAsync<T>(string tableName, string? filter = null) where T : class, ITableEntity;
        Task AddEntityAsync<T>(string tableName, T entity) where T : class, ITableEntity;
        Task UpsertEntityAsync<T>(string tableName, T entity) where T : class, ITableEntity;
        Task DeleteEntityAsync<T>(string tableName, string partitionKey, string rowKey) where T : class, ITableEntity;
    }
}
