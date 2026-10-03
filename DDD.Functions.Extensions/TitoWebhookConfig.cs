using Microsoft.Extensions.Configuration;

namespace DDD.Functions.Extensions
{
    public class TitoWebhookConfig
    {
        public TitoWebhookConfig(IConfiguration config)
        {
            Secret = config["TitoWebhookSecret"];
            ConnectionString = config["TitoWebhookConnectionString"];
            DeDupeTable = config["TitoWebhookDeDupeTable"];
            OrderNotificationQueue = config["TitoWebhookOrderNotificationQueue"];
            TicketNotificationQueue = config["TitoWebhookTicketNotificationQueue"];
            ApiBearerToken = config["TitoApiBearerToken"];
        }

        public string Secret { get; set; }

        public string ConnectionString { get; set; }

        public string DeDupeTable { get; set; }

        public string OrderNotificationQueue { get; set; }

        public string TicketNotificationQueue { get; set; }

        public string ApiBearerToken { get; set; }
    }
}
