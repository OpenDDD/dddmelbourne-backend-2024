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
    public class SessionizeReadModelSync
    {
        private readonly ILogger<SessionizeReadModelSync> log;
        private readonly ConferenceConfig conference;
        private readonly KeyDatesConfig keyDates;
        private readonly SubmissionsConfig submissions;
        private readonly SessionizeSyncConfig sessionize;

        public SessionizeReadModelSync(ILogger<SessionizeReadModelSync> log, ConferenceConfig conference, KeyDatesConfig keyDates, SubmissionsConfig submissions, SessionizeSyncConfig sessionize)
        {
            this.log = log;
            this.conference = conference;
            this.keyDates = keyDates;
            this.submissions = submissions;
            this.sessionize = sessionize;
        }

        [Function("SessionizeReadModelSync")]
        public async Task Run(
            [TimerTrigger("%SessionizeReadModelSyncSchedule%")]
            TimerInfo timer)
        {
            if (keyDates.After(x => x.StopSyncingSessionsFromDate))
            {
                log.LogInformation("SessionizeReadModelSync sync date passed");
                return;
            }

            using (var httpClient = new HttpClient())
            {
                var apiClient = new SessionizeApiClient(httpClient, sessionize.SubmissionsApiKey);
                var (sessionsRepo, presentersRepo) = await submissions.GetRepositoryAsync();

                await SyncService.Sync(apiClient, sessionsRepo, presentersRepo, log, new DateTimeProvider(), conference.ConferenceInstance);
            }
        }
    }
}
