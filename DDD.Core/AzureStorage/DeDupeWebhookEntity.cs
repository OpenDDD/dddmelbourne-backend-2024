namespace DDD.Core.AzureStorage
{
    public class DedupeWebhookEntity : TableStorageEntity
    {
        public DedupeWebhookEntity() {}

        public DedupeWebhookEntity(string webhookType, string eventType, string id)
        {
            PartitionKey = webhookType;
            RowKey = $"{eventType}|{id}";
        }
    }
}
