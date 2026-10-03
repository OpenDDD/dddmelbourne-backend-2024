using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;
using DDD.Functions.Extensions;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace DDD.Functions
{
    public class GetFeedback
    {
        private readonly ILogger<GetFeedback> log;
        private readonly ConferenceConfig conference;
        private readonly FeedbackConfig feedbackConfig;
        private readonly SessionsConfig sessionsConfig;

        public GetFeedback(ILogger<GetFeedback> log, ConferenceConfig conference, FeedbackConfig feedbackConfig, SessionsConfig sessionsConfig)
        {
            this.log = log;
            this.conference = conference;
            this.feedbackConfig = feedbackConfig;
            this.sessionsConfig = sessionsConfig;
        }

        [Function("GetFeedback")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
            HttpRequest req)
        {
            var (conferenceFeedbackRepo, sessionFeedbackRepo) = await feedbackConfig.GetRepositoryAsync();
            var conferenceFeedback = await conferenceFeedbackRepo.GetAllAsync(conference.ConferenceInstance);
            var sessionFeedback = await sessionFeedbackRepo.GetAllAsync(conference.ConferenceInstance);

            var (sessionsRepo, presentersRepo) = await sessionsConfig.GetRepositoryAsync();
            var sessions = await sessionsRepo.GetAllAsync(conference.ConferenceInstance);
            var presenters = await presentersRepo.GetAllAsync(conference.ConferenceInstance);

            var feedback = new
            {
                Conference = conferenceFeedback.OrderBy(x => x.Timestamp).Select(x => new
                {
                    x.Timestamp,
                    x.Rating,
                    x.Liked,
                    x.ImprovementIdeas
                }),
                Sessions = sessions.Select(x => x.GetSession())
                    .Select(s => new
                    {
                        s.Id,
                        s.Title,
                        Presenters = string.Join(", ", s.PresenterIds.Select(pId => presenters.Single(p => p.Id == pId)).Select(x => x.Name)),
                        Feedback = sessionFeedback.OrderBy(x => x.Timestamp).Where(x => x.SessionName.ToLowerInvariant().Contains(s.Title.ToLowerInvariant())).Select(x => new
                        {
                            x.Timestamp,
                            x.Rating,
                            x.Liked,
                            x.ImprovementIdeas
                        })
                    })
            };

            var settings = new JsonSerializerSettings();
            settings.ContractResolver = new DefaultContractResolver();

            return new JsonResult(feedback, settings);
        }
    }
}
