using System;
using Microsoft.Extensions.Configuration;

namespace DDD.Functions.Extensions
{
    public enum TicketNumberWhileVoting
    {
        None,
        Required,
        Optional
    }

    public enum WaitingListCanVoteWithEmail
    {
        False,
        True
    }

    public class VotingConfig
    {
        public VotingConfig(IConfiguration config)
        {
            ConnectionString = config["VotesConnectionString"];
            Table = config["VotingTable"];
            TicketNumberWhileVoting = config["TicketNumberWhileVoting"];
            WaitingListCanVoteWithEmailAppSetting = config["WaitingListCanVoteWithEmail"];
        }

        public string ConnectionString { get; set; }
        public string Table { get; set; }
        public string TicketNumberWhileVoting { get; set; }
        
        public string WaitingListCanVoteWithEmailAppSetting { get; set; }

        public TicketNumberWhileVoting TicketNumberWhileVotingValue =>
            TicketNumberWhileVoting == null ?
                Extensions.TicketNumberWhileVoting.None  :
                (TicketNumberWhileVoting) Enum.Parse(typeof(TicketNumberWhileVoting), TicketNumberWhileVoting);

        public bool WaitingListCanVoteWithEmail => WaitingListCanVoteWithEmailAppSetting != "false";
    }
}
