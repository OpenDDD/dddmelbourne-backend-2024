using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Data.Tables;

namespace DDD.Core.AzureStorage
{
    public interface ITableStorageRepository<T> where T : class, ITableEntity, new()
    {
        Task InitializeAsync();
        Task<T> GetAsync(string partitionKey, string rowKey);
        Task<IList<T>> GetAllAsync(string partitionKey = null, string rowKey = null);
        Task CreateAsync(T item);
        Task UpdateAsync(T item);
        Task DeleteAsync(string partitionKey, string rowKey);
        Task CreateBatchAsync(IList<T> batch);
    }

    public class TableStorageRepository<T> : ITableStorageRepository<T> where T : class, ITableEntity, new()
    {
        private readonly TableClient _table;

        public TableStorageRepository(TableServiceClient client, string tableName)
        {
            _table = client.GetTableClient(tableName);
        }

        public async Task InitializeAsync()
        {
            await _table.CreateIfNotExistsAsync();
        }

        public async Task<T> GetAsync(string partitionKey, string rowKey)
        {
            var record = await _table.GetEntityIfExistsAsync<T>(partitionKey, rowKey);
            return record.HasValue ? record.Value : null;
        }

        public async Task<IList<T>> GetAllAsync(string partitionKey = null, string rowKey = null)
        {
            var filters = new List<string>();
            var returnList = new List<T>();

            if (partitionKey != null)
                filters.Add(TableClient.CreateQueryFilter($"PartitionKey eq {partitionKey}"));

            if (rowKey != null)
                filters.Add(TableClient.CreateQueryFilter($"RowKey eq {rowKey}"));

            await foreach (var entity in _table.QueryAsync<T>(filters.Count == 0 ? null : string.Join(" and ", filters)))
                returnList.Add(entity);

            return returnList;
        }

        public async Task CreateAsync(T item)
        {
            await _table.AddEntityAsync(item);
        }

        public async Task CreateBatchAsync(IList<T> batch)
        {
            if (batch.Count == 0)
                return;

            if (batch.Count > 100)
                throw new InvalidOperationException($"Attempt to insert batch operation with too many records ({batch.Count}), max of 100.");
            if (batch.Any(x => x.PartitionKey != batch[0].PartitionKey))
                throw new InvalidOperationException("Attempt to insert batch operation with records that have a mix of partition keys.");

            await _table.SubmitTransactionAsync(batch.Select(x => new TableTransactionAction(TableTransactionActionType.Add, x)));
        }

        public async Task UpdateAsync(T item)
        {
            await _table.UpdateEntityAsync(item, item.ETag, TableUpdateMode.Replace);
        }

        public async Task DeleteAsync(string partitionKey, string rowKey)
        {
            var existing = await GetAsync(partitionKey, rowKey);
            await _table.DeleteEntityAsync(existing.PartitionKey, existing.RowKey, existing.ETag);
        }
    }
}
