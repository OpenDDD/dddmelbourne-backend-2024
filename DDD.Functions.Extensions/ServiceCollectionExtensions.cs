using Microsoft.Extensions.DependencyInjection;

namespace DDD.Functions.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDddConfig(this IServiceCollection services)
        {
            return services
                .AddSingleton<AgendaScheduleConfig>()
                .AddSingleton<AppInsightsSyncConfig>()
                .AddSingleton<ConferenceConfig>()
                .AddSingleton<EloVotingConfig>()
                .AddSingleton<FeedbackConfig>()
                .AddSingleton<KeyDatesConfig>()
                .AddSingleton<NewSessionNotificationConfig>()
                .AddSingleton<SessionizeSyncConfig>()
                .AddSingleton<SessionsConfig>()
                .AddSingleton<SubmissionsConfig>()
                .AddSingleton<TitoSyncConfig>()
                .AddSingleton<TitoWebhookConfig>()
                .AddSingleton<VotingConfig>();
        }
    }
}
