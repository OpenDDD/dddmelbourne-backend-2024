
using System.Runtime.Serialization;
using DDD.Core.AzureStorage;

namespace DDD.Core.Tito
{
    public class TitoTicket : TableStorageEntity
    {
        public TitoTicket()
        {
        }

        public TitoTicket(string conferenceInstance, string ticketId)
        {
            PartitionKey = conferenceInstance;
            RowKey = ticketId;
        }

        [IgnoreDataMember]
        public string TicketId => RowKey;
    }
}
