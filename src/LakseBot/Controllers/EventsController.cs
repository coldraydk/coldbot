using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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

    public partial class SlackRequest
    {
        [JsonProperty("token")]
        public string Token { get; set; }
        [JsonProperty("challenge")]
        public string Challenge { get; set; }
        [JsonProperty("team_id")]
        public string TeamId { get; set; }
        [JsonProperty("api_app_id")]
        public string ApiAppId { get; set; }
        [JsonProperty("event")]
        public Event Event { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("authed_users")]
        public string[] AuthedUsers { get; set; }
        [JsonProperty("event_id")]
        public string EventId { get; set; }
        [JsonProperty("event_time")]
        public long EventTime { get; set; }
    }

    public partial class Event
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("bot_id")]
        public string BotId { get; set; }
        [JsonProperty("user")]
        public string User { get; set; }
        [JsonProperty("item")]
        public Item Item { get; set; }
        [JsonProperty("reaction")]
        public string Reaction { get; set; }
        [JsonProperty("item_user")]
        public string ItemUser { get; set; }
        [JsonProperty("event_ts")]
        public string EventTs { get; set; }
        [JsonProperty("text")]
        public string Text { get; set; }
        [JsonProperty("channel")]
        public string Channel { get; set; }
    }

    public partial class Item
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("channel")]
        public string Channel { get; set; }
        [JsonProperty("ts")]
        public string Ts { get; set; }
    }
}
