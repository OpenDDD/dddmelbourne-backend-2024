using System.Threading.Tasks;
using Azure.Data.Tables;
using DDD.Core.AppInsights;
using DDD.Core.AzureStorage;
using DDD.Core.Tito;
using DDD.Core.Voting;
using DDD.Core.EloVoting;
using Microsoft.Azure.Cosmos;

namespace DDD.Functions.Extensions
{
    public static class SessionData
    {
        private static readonly string DefaultSessionTimeoutSeconds = "259200";

        public static async Task<ITableStorageRepository<NotifiedSessionEntity>> GetRepositoryAsync(this NewSessionNotificationConfig config)
        {
            var repo = new TableStorageRepository<NotifiedSessionEntity>(new TableServiceClient(config.ConnectionString), config.Table);
            await repo.InitializeAsync();
            return repo;
        }

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

        public static async Task<(ITableStorageRepository<DedupeWebhookEntity>, IQueueStorageRepository<OrderNotificationEvent>, IQueueStorageRepository<TicketNotificationEvent>)> GetRepositoryAsync(this TitoWebhookConfig config)
        {
            var deDupeRepository = new TableStorageRepository<DedupeWebhookEntity>(new TableServiceClient(config.ConnectionString), config.DeDupeTable);
            var orderNotificationQueue = new QueueStorageRepository<OrderNotificationEvent>(config.ConnectionString, config.OrderNotificationQueue);
            var ticketNotificationQueue = new QueueStorageRepository<TicketNotificationEvent>(config.ConnectionString, config.TicketNotificationQueue);
            await deDupeRepository.InitializeAsync();
            await orderNotificationQueue.InitializeAsync();
            await ticketNotificationQueue.InitializeAsync();
            return (deDupeRepository, orderNotificationQueue, ticketNotificationQueue);
        }

        public static async Task<(ITableStorageRepository<TitoTicket>, ITableStorageRepository<WaitingList>)> GetRepositoryAsync(this TitoSyncConfig config)
        {
            var repoTickets = new TableStorageRepository<TitoTicket>(new TableServiceClient(config.ConnectionString), config.Table);
            var repoWaitingListEmails = new TableStorageRepository<WaitingList>(new TableServiceClient(config.WaitinglistConnectionString), config.WaitingListTable);
            await repoTickets.InitializeAsync();
            await repoWaitingListEmails.InitializeAsync();
            return (repoTickets, repoWaitingListEmails);
        }

        public static async Task<ITableStorageRepository<AppInsightsVotingUser>> GetRepositoryAsync(this AppInsightsSyncConfig config)
        {
            var repo = new TableStorageRepository<AppInsightsVotingUser>(new TableServiceClient(config.ConnectionString), config.Table);
            await repo.InitializeAsync();
            return repo;
        }

        public static async Task<ITableStorageRepository<Vote>> GetRepositoryAsync(this VotingConfig config)
        {
            var repo = new TableStorageRepository<Vote>(new TableServiceClient(config.ConnectionString), config.Table);
            await repo.InitializeAsync();
            return repo;
        }

        public static async Task<ITableStorageRepository<EloVote>> GetRepositoryAsync(this EloVotingConfig config)
        {
            var repo = new TableStorageRepository<EloVote>(new TableServiceClient(config.ConnectionString), config.Table);
            await repo.InitializeAsync();
            return repo;
        }

        public static async Task<(ITableStorageRepository<ConferenceFeedbackEntity>, ITableStorageRepository<SessionFeedbackEntity>)> GetRepositoryAsync(this FeedbackConfig feedback)
        {
            var tableClient = new TableServiceClient(feedback.ConnectionString);
            var conferenceFeedbackRepository = new TableStorageRepository<ConferenceFeedbackEntity>(tableClient, feedback.ConferenceFeedbackTable);
            var sessionFeedbackRepository = new TableStorageRepository<SessionFeedbackEntity>(tableClient, feedback.SessionFeedbackTable);
            await conferenceFeedbackRepository.InitializeAsync();
            await sessionFeedbackRepository.InitializeAsync();

            return (conferenceFeedbackRepository, sessionFeedbackRepository);
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
