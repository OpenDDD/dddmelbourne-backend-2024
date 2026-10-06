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
