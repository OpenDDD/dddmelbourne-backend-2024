using System.Net.Http;
using System.Threading.Tasks;
using DDD.Core.Time;
using DDD.Functions.Extensions;
using DDD.Sessionize.Sessionize;
using DDD.Sessionize.Sync;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DDD.Functions
{
    public class SessionizeAgendaSync
    {
        private readonly ILogger<SessionizeAgendaSync> log;
        private readonly ConferenceConfig conference;
        private readonly KeyDatesConfig keyDates;
        private readonly SessionsConfig sessions;
        private readonly SessionizeSyncConfig sessionize;

        public SessionizeAgendaSync(ILogger<SessionizeAgendaSync> log, ConferenceConfig conference, KeyDatesConfig keyDates, SessionsConfig sessions, SessionizeSyncConfig sessionize)
        {
            this.log = log;
            this.conference = conference;
            this.keyDates = keyDates;
            this.sessions = sessions;
            this.sessionize = sessionize;
        }

        [Function("SessionizeAgendaSync")]
        public async Task Run(
            [TimerTrigger("%SessionizeReadModelSyncSchedule%")]
            TimerInfo timer)
        {
            if (keyDates.Before(x => x.StopSyncingSessionsFromDate) || keyDates.After(x => x.StopSyncingAgendaFromDate))
            {
                log.LogInformation("SessionizeAgendaSync sync not active");
                return;
            }

            using (var httpClient = new HttpClient())
            {
                var apiClient = new SessionizeApiClient(httpClient, sessionize.AgendaApiKey);
                var (sessionsRepo, presentersRepo) = await sessions.GetRepositoryAsync();

                await SyncService.Sync(apiClient, sessionsRepo, presentersRepo, log, new DateTimeProvider(), conference.ConferenceInstance);
            }
        }
    }
}
