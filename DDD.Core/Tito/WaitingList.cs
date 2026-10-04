
using System.Runtime.Serialization;
using DDD.Core.AzureStorage;

namespace DDD.Core.Tito
{
    public class WaitingList : TableStorageEntity
    {
        public WaitingList()
        {
        }

        public WaitingList(string conferenceInstance, string email)
        {
            PartitionKey = conferenceInstance;
            RowKey = email;
        }

        [IgnoreDataMember]
        public string Email => RowKey;
    }
}
