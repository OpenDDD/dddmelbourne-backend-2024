using Microsoft.Extensions.Configuration;

namespace DDD.Functions.Extensions
{
    public class AppInsightsSyncConfig
    {
        public AppInsightsSyncConfig(IConfiguration config)
        {
            ConnectionString = config["VotesConnectionString"];
            Table = config["AppInsightsTable"];
            ApplicationId = config["AppInsightsApplicationId"];
            ApplicationKey = config["AppInsightsApplicationKey"];
        }

        public string ConnectionString { get; set; }

        public string Table { get; set; }

        public string ApplicationId { get; set; }

        public string ApplicationKey { get; set; }
    }
}
