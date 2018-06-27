using System;
using Newtonsoft.Json;

namespace ColdBot.Models {

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
    
}