using System;
using Azure;
using Azure.Data.Tables;
using Newtonsoft.Json;

namespace DDD.Core.AzureStorage
{
    // Azure.Data.Tables writes get-only properties, but Microsoft.Azure.Cosmos.Table did not.
    // Mark computed properties in derived entities with [IgnoreDataMember] to keep the stored columns the same.
    public abstract class TableStorageEntity : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }

        // Preserve the string representation returned by the previous table SDK.
        [JsonConverter(typeof(ETagJsonConverter))]
        public ETag ETag { get; set; }
    }

    public class ETagJsonConverter : JsonConverter<ETag>
    {
        public override void WriteJson(JsonWriter writer, ETag value, JsonSerializer serializer)
        {
            if (value == default)
                writer.WriteNull();
            else
                writer.WriteValue(value.ToString());
        }

        public override ETag ReadJson(JsonReader reader, Type objectType, ETag existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return reader.Value is string value ? new ETag(value) : default;
        }
    }
}
