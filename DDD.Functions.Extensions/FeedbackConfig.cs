using Microsoft.Extensions.Configuration;

namespace DDD.Functions.Extensions
{
    public class FeedbackConfig
    {
        public FeedbackConfig(IConfiguration config)
        {
            ConnectionString = config["FeedbackConnectionString"];
            SessionFeedbackTable = config["SessionFeedbackTable"];
            ConferenceFeedbackTable = config["ConferenceFeedbackTable"];
            IsSingleVoteEligibleForPrizeDrawAppSetting = config["IsSingleVoteEligibleForPrizeDraw"];
        }

        public string ConnectionString { get; set; }
        public string SessionFeedbackTable { get; set; }
        public string ConferenceFeedbackTable { get; set; }
        public string IsSingleVoteEligibleForPrizeDrawAppSetting { get; set; }
        public bool IsSingleVoteEligibleForPrizeDraw => IsSingleVoteEligibleForPrizeDrawAppSetting != "false";
    }
}
