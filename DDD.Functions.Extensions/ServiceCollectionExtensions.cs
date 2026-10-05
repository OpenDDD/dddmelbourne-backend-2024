using Microsoft.Extensions.DependencyInjection;

namespace DDD.Functions.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDddConfig(this IServiceCollection services)
        {
            return services
                .AddSingleton<AgendaScheduleConfig>()
                .AddSingleton<ConferenceConfig>()
                .AddSingleton<EloVotingConfig>()
                .AddSingleton<KeyDatesConfig>()
                .AddSingleton<SessionizeSyncConfig>()
                .AddSingleton<SessionsConfig>()
                .AddSingleton<SubmissionsConfig>();
        }
    }
}
