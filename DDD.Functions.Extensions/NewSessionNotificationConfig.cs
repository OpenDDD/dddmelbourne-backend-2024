using Microsoft.Extensions.Configuration;

namespace DDD.Functions.Extensions
{
    public class NewSessionNotificationConfig
    {
        public NewSessionNotificationConfig(IConfiguration config)
        {
            ConnectionString = config["SessionsConnectionString"];
            Table = config["NotifiedSessionsTable"];
            LogicAppUrl = config["NewSessionNotificationLogicAppUrl"];
        }

        public string ConnectionString { get; set; }
        public string Table { get; set; }
        public string LogicAppUrl { get; set; }
    }
}
