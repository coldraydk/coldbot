using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace LakseBot.Services {
    public class SlackService 
    {
        private const string SLACK_URL = "https://slack.com/api/chat.postMessage";
        private static HttpClient client = new HttpClient();
        private string botToken; 

        public SlackService() 
        {
            botToken = System.Environment.GetEnvironmentVariable("BOT_TOKEN");
        }

        public async void SendMessage(string text, string channel)
        {
            var parameters = new Dictionary<string, string>();
            parameters.Add("token", System.Environment.GetEnvironmentVariable("BOT_TOKEN"));
            parameters.Add("text", text);
            parameters.Add("channel", channel);

            var slackResponse = await client.PostAsync(SLACK_URL, new FormUrlEncodedContent(parameters));
        }
    }
}