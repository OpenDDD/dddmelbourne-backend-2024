using System.Threading.Tasks;
using Azure.Storage.Queues;
using Newtonsoft.Json;

namespace DDD.Core.AzureStorage
{
    public interface IQueueStorageRepository<T> where T : class, new()
    {
        Task InitializeAsync();
        Task PushAsync(T item);
    }

    public class QueueStorageRepository<T> : IQueueStorageRepository<T> where T : class, new()
    {
        private readonly QueueClient _queue;

        public QueueStorageRepository(string connectionString, string queueName)
        {
            // Microsoft.Azure.Storage.Queue base64-encoded messages, and the Logic Apps that read the queue expect that.
            _queue = new QueueClient(connectionString, queueName, new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 });
        }

        public async Task InitializeAsync()
        {
            await _queue.CreateIfNotExistsAsync();
        }

        public async Task PushAsync(T item)
        {
            await _queue.SendMessageAsync(JsonConvert.SerializeObject(item));
        }
    }
}
