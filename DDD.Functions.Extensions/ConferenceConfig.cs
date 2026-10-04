using System;
using Microsoft.Extensions.Configuration;

namespace DDD.Functions.Extensions
{
    public class ConferenceConfig
    {
        public ConferenceConfig(IConfiguration config)
        {
            ConferenceInstance = config["ConferenceInstance"];
            AnonymousSubmissionsAppSetting = config["AnonymousSubmissions"];
            MinVotesSetting = config["MinVotes"];
            MaxVotesSetting = config["MaxVotes"];
            MinNumSessionFeedbackForPrizeDrawSetting = config["MinNumSessionFeedbackForPrizeDraw"];
        }

        public string ConferenceInstance { get; set; }

        // Anonymous submissions
        public string AnonymousSubmissionsAppSetting { get; set; }
        public bool AnonymousSubmissions => AnonymousSubmissionsAppSetting != "false";

        // Min votes
        public string MinVotesSetting { get; set; }
        public int MinVotes => MinVotesSetting != null ? Int32.Parse(MinVotesSetting) : 0;

        public string MaxVotesSetting { get; set; }
        public int MaxVotes => MaxVotesSetting != null ? Int32.Parse(MaxVotesSetting) : 0;

        // Min session feedback for prize draw
        public string MinNumSessionFeedbackForPrizeDrawSetting { get; set; }
        public int MinNumSessionFeedbackForPrizeDraw => MinNumSessionFeedbackForPrizeDrawSetting != null ? Int32.Parse(MinNumSessionFeedbackForPrizeDrawSetting) : 0;
    }
}
