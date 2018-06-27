using Newtonsoft.Json;

namespace ColdBot.Models
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