using Newtonsoft.Json;

namespace ColdBot.Models {
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
}