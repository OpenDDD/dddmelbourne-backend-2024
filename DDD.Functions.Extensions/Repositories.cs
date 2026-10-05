using System.Threading.Tasks;
using Azure.Data.Tables;
using DDD.Core.AzureStorage;
using DDD.Core.EloVoting;
using Microsoft.Azure.Cosmos;

namespace DDD.Functions.Extensions
{
    public static class SessionData
    {
        private static readonly string DefaultSessionTimeoutSeconds = "259200";

        public static Task<(ITableStorageRepository<SessionEntity>, ITableStorageRepository<PresenterEntity>)> GetRepositoryAsync(this SubmissionsConfig config)
        {
            return GetSessionRepositoryAsync(config.ConnectionString, config.SubmissionsTable, config.SubmittersTable);
        }

        private static CosmosClient _cosmosClient;
        private static readonly object _cosmosClientLock = new object();
        private static CosmosClient GetCosmosClient(string connectionString)
        {
            if (_cosmosClient != null)
            {
                return _cosmosClient;
            }

            lock (_cosmosClientLock)
            {
                return _cosmosClient ??= new CosmosClient(connectionString, new CosmosClientOptions());
            } 
        }
        
        public static async Task<IUserVotingSessionRepository> GetUserVoteSessionRepositoryAsync(this SubmissionsConfig config)
        {
            var client = GetCosmosClient(config.UserVotingSessionsString);            
            var repo = new UserVotingSessionRepository(client, long.Parse(string.IsNullOrEmpty(config.UserVotingSessionTtlSeconds) ? DefaultSessionTimeoutSeconds : config.UserVotingSessionTtlSeconds));

            await repo.InitialiseAsync(config.UserVotingSessionsDatabaseId, config.UserVotingSessionsContainerId);

            return repo;
        }

        public static Task<(ITableStorageRepository<SessionEntity>, ITableStorageRepository<PresenterEntity>)> GetRepositoryAsync(this SessionsConfig config)
        {
            return GetSessionRepositoryAsync(config.ConnectionString, config.SessionsTable, config.PresentersTable);
        }

        public static async Task<ITableStorageRepository<EloVote>> GetRepositoryAsync(this EloVotingConfig config)
        {
            var repo = new TableStorageRepository<EloVote>(new TableServiceClient(config.ConnectionString), config.Table);
            await repo.InitializeAsync();
            return repo;
        }

        private static async Task<(ITableStorageRepository<SessionEntity>, ITableStorageRepository<PresenterEntity>)> GetSessionRepositoryAsync(string connectionString, string sessionsTable, string presentersTable)
        {
            var tableClient = new TableServiceClient(connectionString);
            var sessionsRepo = new TableStorageRepository<SessionEntity>(tableClient, sessionsTable);
            var presentersRepo = new TableStorageRepository<PresenterEntity>(tableClient, presentersTable);
            await sessionsRepo.InitializeAsync();
            await presentersRepo.InitializeAsync();

            return (sessionsRepo, presentersRepo);
        }
    }
}
