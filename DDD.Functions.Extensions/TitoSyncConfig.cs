using Microsoft.Extensions.Configuration;

namespace DDD.Functions.Extensions
{
    public class TitoSyncConfig
    {
        public TitoSyncConfig(IConfiguration config)
        {
            ConnectionString = config["VotesConnectionString"];
            Table = config["TitoTable"];
            ApiKey = config["TitoApiBearerToken"];
            EventId = config["TitoEventId"];
            AccountId = config["TitoAccountId"];
            WaitinglistConnectionString = config["WaitinglistConnectionString"];
            WaitingListTable = config["WaitingListTable"];
        }

        public string ConnectionString { get; set; }
        public string Table { get; set; }
        public string ApiKey { get; set; }
        public string EventId { get; set; }
        public string AccountId { get; set; }
        public string WaitinglistConnectionString { get; set; }
        public string WaitingListTable { get; set; }
    }
}
