using Microsoft.Extensions.Configuration;

namespace DDD.Functions.Extensions
{
    public class AgendaScheduleConfig
    {
        public AgendaScheduleConfig(IConfiguration config)
        {
            ConnectionString = config["AgendaScheduleConnectionString"];
            Container = config["AgendaScheduleContainer"];
        }

        public string ConnectionString { get; set; }
        public string Container { get; set; }
    }
}
