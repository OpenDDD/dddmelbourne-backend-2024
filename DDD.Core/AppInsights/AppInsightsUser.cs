using System;
using System.Collections.Generic;
using DDD.Core.AzureStorage;

namespace DDD.Core.AppInsights
{
    public class AppInsightsVotingUser : TableStorageEntity
    {
        public AppInsightsVotingUser() { }

        public AppInsightsVotingUser(string conferenceInstance, string userId, string voteId, string startTime)
        {
            PartitionKey = conferenceInstance;
            RowKey = Guid.NewGuid().ToString();
            UserId = userId;
            VoteId = voteId;
            StartTime = startTime;
        }

        public string UserId { get; set; }
        public string VoteId { get; set; }
        public string StartTime { get; set; }
    }

    public class AppInsightsVotingUserComparer : IEqualityComparer<AppInsightsVotingUser>
    {
        public bool Equals(AppInsightsVotingUser x, AppInsightsVotingUser y)
        {
            return x.UserId == y.UserId && x.VoteId == y.VoteId;
        }

        public int GetHashCode(AppInsightsVotingUser obj)
        {
            return (obj.UserId + "|" + obj.VoteId).GetHashCode();
        }
    }
}
