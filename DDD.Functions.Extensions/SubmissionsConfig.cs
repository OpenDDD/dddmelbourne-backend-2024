using Microsoft.Extensions.Configuration;

namespace DDD.Functions.Extensions
{
    public class SubmissionsConfig
    {
        public SubmissionsConfig(IConfiguration config)
        {
            ConnectionString = config["SessionsConnectionString"];
            SubmissionsTable = config["SubmissionsTable"];
            SubmittersTable = config["SubmittersTable"];
            UserVotingSessionsString = config["UserVotingSessionsConnectionString"];
            UserVotingSessionsDatabaseId = config["UserVotingSessionsDatabaseId"];
            UserVotingSessionsContainerId = config["UserVotingSessionsContainerId"];
            UserVotingSessionHeaderName = config["UserVotingSessionHeaderName"];
            UserVotingSessionTtlSeconds = config["UserVotingSessionTtlSeconds"];
        }

        public string ConnectionString { get; set; }
        public string SubmissionsTable { get; set; }
        public string SubmittersTable { get; set; }
        public string UserVotingSessionsString { get; set; }
        public string UserVotingSessionsDatabaseId { get; set; }
        public string UserVotingSessionsContainerId { get; set; }
        public string UserVotingSessionHeaderName { get; set; }
        public string UserVotingSessionTtlSeconds { get; set; }
    }
}
