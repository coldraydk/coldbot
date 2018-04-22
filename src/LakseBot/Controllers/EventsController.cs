using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using LakseBot.Models;

namespace LakseBot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private const string SLACK_URL = "https://slack.com/api/chat.postMessage";
        private static HttpClient client = new HttpClient();
        private ILogger<EventsController> logger;
        private string botToken; 

        public EventsController(ILogger<EventsController> logger)
        {
            botToken = System.Environment.GetEnvironmentVariable("BOT_TOKEN");

            this.logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Hi guys");
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] SlackRequest request)
        {
            logger.LogInformation(JsonConvert.SerializeObject(request));

            if (!string.IsNullOrEmpty(request.Challenge))
            {
                return Ok(request.Challenge);
            }

            if (!string.IsNullOrEmpty(request.Event.BotId))
            {
                return Ok();
            }

            var parameters = new Dictionary<string, string>();
            parameters.Add("token", System.Environment.GetEnvironmentVariable("BOT_TOKEN"));
            parameters.Add("text", string.Join("", request.Event.Text.Reverse()));
            parameters.Add("channel", request.Event.Channel);

            var slackResponse = await client.PostAsync(SLACK_URL, new FormUrlEncodedContent(parameters));

            return Ok();
        }
    }
}
