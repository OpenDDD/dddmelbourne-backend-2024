using Microsoft.Extensions.Configuration;

namespace DDD.Functions.Extensions
{
    public class ConferenceConfig
    {
        public ConferenceConfig(IConfiguration config)
        {
            ConferenceInstance = config["ConferenceInstance"];
            AnonymousSubmissionsAppSetting = config["AnonymousSubmissions"];
        }

        public string ConferenceInstance { get; set; }

        // Anonymous submissions
        public string AnonymousSubmissionsAppSetting { get; set; }
        public bool AnonymousSubmissions => AnonymousSubmissionsAppSetting != "false";
    }
}
