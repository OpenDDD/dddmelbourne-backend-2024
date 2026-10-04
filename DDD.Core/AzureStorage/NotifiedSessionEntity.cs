using System;
using System.Runtime.Serialization;

namespace DDD.Core.AzureStorage
{
    public class NotifiedSessionEntity : TableStorageEntity
    {
        public NotifiedSessionEntity() {}

        public NotifiedSessionEntity(Guid id)
        {
            PartitionKey = id.ToString();
            RowKey = Guid.NewGuid().ToString();
        }

        [IgnoreDataMember]
        public Guid Id => Guid.Parse(PartitionKey);
    }
}
