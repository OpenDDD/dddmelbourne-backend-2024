using Microsoft.Azure.Cosmos.Table;
namespace DDD.Core.EloVoting
{
    public class EloVote : TableEntity
    {
        public EloVote() { }

        public EloVote(string conferenceInstance, string voteId, string winnerSessionId, string loserSessionId, bool isDraw, string ipAddress, string voterSessionId, string voterTicket, string voterLastname)
        {
            PartitionKey = conferenceInstance;
            RowKey = voteId;
            WinnerSessionId = winnerSessionId;
            LoserSessionId = loserSessionId;
            IsDraw = isDraw;
            IpAddress = ipAddress;
            VoterSessionId = voterSessionId;
            VoterTicket = voterTicket;
            VoterLastname = voterLastname;
        }

        public string WinnerSessionId { get; set; }
        public string LoserSessionId { get; set; }
        public bool IsDraw { get; set; }
        public string IpAddress { get; set; }
        public string VoterSessionId { get; set; }
        public string VoterTicket { get; set; }
        public string VoterLastname { get; set; }
        public string VoteId => RowKey;
    }
}
