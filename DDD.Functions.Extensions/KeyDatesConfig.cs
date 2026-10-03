using System;
using Microsoft.Extensions.Configuration;

namespace DDD.Functions.Extensions
{
    public class KeyDatesConfig
    {
        public KeyDatesConfig(IConfiguration config)
        {
            StopSyncingSessionsFrom = config["StopSyncingSessionsFrom"];
            StopSyncingAgendaFrom = config["StopSyncingAgendaFrom"];
            SubmissionsAvailableFrom = config["SubmissionsAvailableFrom"];
            SubmissionsAvailableTo = config["SubmissionsAvailableTo"];
            VotingAvailableFrom = config["VotingAvailableFrom"];
            VotingAvailableTo = config["VotingAvailableTo"];
            StartSyncingAppInsightsFrom = config["StartSyncingAppInsightsFrom"];
            StopSyncingAppInsightsFrom = config["StopSyncingAppInsightsFrom"];
            StopSyncingTitoFrom = config["StopSyncingTitoFrom"];
            FeedbackAvailableFrom = config["FeedbackAvailableFrom"];
            FeedbackAvailableTo = config["FeedbackAvailableTo"];
        }

        public DateTimeOffset Now => DateTimeOffset.UtcNow;

        // Sessionize Sync
        public string StopSyncingSessionsFrom { get; set; }
        public DateTimeOffset StopSyncingSessionsFromDate => StopSyncingSessionsFrom != null ? DateTimeOffset.Parse(StopSyncingSessionsFrom) : DateTimeOffset.MinValue;
        public string StopSyncingAgendaFrom { get; set; }
        public DateTimeOffset StopSyncingAgendaFromDate => StopSyncingAgendaFrom != null ? DateTimeOffset.Parse(StopSyncingAgendaFrom) : DateTimeOffset.MinValue;

        // Submissions
        public string SubmissionsAvailableFrom { get; set; }
        public DateTimeOffset SubmissionsAvailableFromDate => SubmissionsAvailableFrom != null ? DateTimeOffset.Parse(SubmissionsAvailableFrom) : DateTimeOffset.MaxValue;
        public string SubmissionsAvailableTo { get; set; }
        public DateTimeOffset SubmissionsAvailableToDate => SubmissionsAvailableTo != null ? DateTimeOffset.Parse(SubmissionsAvailableTo) : DateTimeOffset.MinValue;

        // Voting
        public string VotingAvailableFrom { get; set; }
        public DateTimeOffset VotingAvailableFromDate => VotingAvailableFrom != null ? DateTimeOffset.Parse(VotingAvailableFrom) : DateTimeOffset.MaxValue;
        public string VotingAvailableTo { get; set; }
        public DateTimeOffset VotingAvailableToDate => VotingAvailableTo != null ? DateTimeOffset.Parse(VotingAvailableTo) : DateTimeOffset.MinValue;

        // App Insights Sync
        public string AppInsightsApplicationKey { get; set; }
        public string StartSyncingAppInsightsFrom { get; set; }
        public DateTimeOffset StartSyncingAppInsightsFromDate => StartSyncingAppInsightsFrom != null ? DateTimeOffset.Parse(StartSyncingAppInsightsFrom) : DateTimeOffset.MinValue;
        public string StopSyncingAppInsightsFrom { get; set; }
        public DateTimeOffset StopSyncingAppInsightsFromDate => StopSyncingAppInsightsFrom != null ? DateTimeOffset.Parse(StopSyncingAppInsightsFrom) : DateTimeOffset.MinValue;

        // Tito Sync
        public string StopSyncingTitoFrom { get; set; }
        public DateTimeOffset StopSyncingTitoFromDate => StopSyncingTitoFrom != null ? DateTimeOffset.Parse(StopSyncingTitoFrom) : DateTimeOffset.MinValue;

        // Feedback
        public string FeedbackAvailableFrom { get; set; }
        public DateTimeOffset FeedbackAvailableFromDate => FeedbackAvailableFrom != null ? DateTimeOffset.Parse(FeedbackAvailableFrom) : DateTimeOffset.MinValue;
        public string FeedbackAvailableTo { get; set; }
        public DateTimeOffset FeedbackAvailableToDate => FeedbackAvailableTo != null ? DateTimeOffset.Parse(FeedbackAvailableTo) : DateTimeOffset.MaxValue;
        public bool Before(Func<KeyDatesConfig, DateTimeOffset> date, TimeSpan? tolerance = null)
        {
            return Now < date(this).Add(tolerance ?? TimeSpan.Zero);
        }

        public bool After(Func<KeyDatesConfig, DateTimeOffset> date, TimeSpan? tolerance = null)
        {
            return Now > date(this).Add(tolerance ?? TimeSpan.Zero);
        }
    }
}
