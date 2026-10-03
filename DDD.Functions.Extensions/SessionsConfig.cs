using Microsoft.Extensions.Configuration;

namespace DDD.Functions.Extensions
{
    public class SessionsConfig
    {
        public SessionsConfig(IConfiguration config)
        {
            ConnectionString = config["SessionsConnectionString"];
            SessionsTable = config["SessionsTable"];
            PresentersTable = config["PresentersTable"];
        }

        public string ConnectionString { get; set; }
        public string SessionsTable { get; set; }
        public string PresentersTable { get; set; }
    }
}
