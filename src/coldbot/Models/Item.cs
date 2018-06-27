using Newtonsoft.Json;

namespace LakseBot.Models {
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