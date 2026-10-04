using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;
using DDD.Functions.Extensions;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DDD.Functions
{
    public class GetPrizeDraw
    {
        private readonly ILogger<GetPrizeDraw> log;
        private readonly ConferenceConfig conference;
        private readonly FeedbackConfig feedbackConfig;

        public GetPrizeDraw(ILogger<GetPrizeDraw> log, ConferenceConfig conference, FeedbackConfig feedbackConfig)
        {
            this.log = log;
            this.conference = conference;
            this.feedbackConfig = feedbackConfig;
        }

        [Function("GetPrizeDraw")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
            HttpRequest req)
        {
            var (conferenceFeedbackRepo, sessionFeedbackRepo) = await feedbackConfig.GetRepositoryAsync();
            var conferenceFeedback = await conferenceFeedbackRepo.GetAllAsync(conference.ConferenceInstance);
            var sessionFeedback = await sessionFeedbackRepo.GetAllAsync(conference.ConferenceInstance);

            string [] prizeDraw;
            if(feedbackConfig.IsSingleVoteEligibleForPrizeDraw)
            {
                var conferenceFeedbackCandidates = conferenceFeedback.Any()? conferenceFeedback.Select(x => x.Name): new List<string>();
                var sessionsFeedbackCandidates = sessionFeedback.Any()? sessionFeedback.Select(x => x.Name): new List<string>();
                prizeDraw = conferenceFeedbackCandidates.Concat(sessionsFeedbackCandidates).ToArray();
            } 
            else 
            {
                prizeDraw = conferenceFeedback.Select(x => x.Name)
                    .Where(name =>
                        sessionFeedback.Count(s => s.Name.ToLowerInvariant() == name.ToLowerInvariant()) >=
                        conference.MinNumSessionFeedbackForPrizeDraw)
                    .ToArray();
            }

            var settings = new JsonSerializerSettings();
            settings.ContractResolver = new DefaultContractResolver();

            return new JsonResult(prizeDraw, settings);
        }
    }
}
