using System;
using System.Runtime.Serialization;
using DDD.Core.AzureStorage;
using Newtonsoft.Json;

namespace DDD.Core.Voting
{
    public class Vote : TableStorageEntity
    {
        public Vote() { }

        public Vote(string conferenceInstance, Guid voteId, string[] sessionIds, int[] indices, string ticketNumber, string ipAddress, string voterSessionId, DateTimeOffset votingStartTime, DateTimeOffset votingSubmittedTime)
        {
            PartitionKey = conferenceInstance;
            RowKey = voteId.ToString();
            SessionIds = JsonConvert.SerializeObject(sessionIds);
            Indices = JsonConvert.SerializeObject(indices);
            TicketNumber = ticketNumber;
            IpAddress = ipAddress;
            VoterSessionId = voterSessionId;
            VotingStartTime = votingStartTime;
            VotingSubmittedTime = votingSubmittedTime;
        }

        public string SessionIds { get; set; }

        public string[] GetSessionIds()
        {
            return JsonConvert.DeserializeObject<string[]>(SessionIds);
        }

        public string Indices { get; set; }

        public string[] GetIndices()
        {
            return JsonConvert.DeserializeObject<string[]>(Indices);
        }

        public string TicketNumber { get; set; }
        public string IpAddress { get; set; }
        public string VoterSessionId { get; set; }
        public DateTimeOffset VotingStartTime { get; set; }
        public DateTimeOffset VotingSubmittedTime { get; set; }
        [IgnoreDataMember]
        public string VoteId => RowKey;

        // GetVotes returns this entity as JSON. Newtonsoft skips [IgnoreDataMember], and Azure.Data.Tables skips non-public members.
        [JsonProperty(nameof(VoteId))]
        private string SerializedVoteId => RowKey;
    }
}
