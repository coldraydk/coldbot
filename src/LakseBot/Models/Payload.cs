using Newtonsoft.Json;

namespace LakseBot.Models
{
    public class Payload
    {
        [JsonProperty("channel")]
        public string Channel { get; set; }
        
        [JsonProperty("token")]
        public string Token { get; set; }
        
        [JsonProperty("text")]
        public string Text { get; set; }
    } 
}