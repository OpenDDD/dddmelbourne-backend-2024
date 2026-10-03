using Microsoft.Extensions.Configuration;

namespace DDD.Functions.Extensions
{
    public class SessionizeSyncConfig
    {
        public SessionizeSyncConfig(IConfiguration config)
        {
            SubmissionsApiKey = config["SessionizeApiKey"];
            AgendaApiKey = config["SessionizeAgendaApiKey"];
        }

        public string SubmissionsApiKey { get; set; }

        public string AgendaApiKey { get; set; }
    }
}
