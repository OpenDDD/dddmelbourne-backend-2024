using System;
using Microsoft.Extensions.Configuration;

namespace DDD.Functions.Extensions
{

    public class EloVotingConfig
    {
        public EloVotingConfig(IConfiguration config)
        {
            ConnectionString = config["EloVotesConnectionString"];
            Table = config["EloVotingTable"];
            EloPasswordPhrase = config["EloPasswordPhrase"];
            EloAllowedTimeInSecondsToSubmitSetting = config["EloAllowedTimeInSecondsToSubmit"];
            EloEnabledSetting = config["EloEnabled"];
        }

        public string ConnectionString { get; set; }

        public string Table { get; set; }

        public string EloPasswordPhrase { get; set; }

        public string EloAllowedTimeInSecondsToSubmitSetting { get; set; }
        public int EloAllowedTimeInSecondsToSubmit => EloAllowedTimeInSecondsToSubmitSetting != null ? Int32.Parse(EloAllowedTimeInSecondsToSubmitSetting) : 0;

        public string EloEnabledSetting { get; set; }
        public bool EloEnabled => EloEnabledSetting != "false";

    }
}
