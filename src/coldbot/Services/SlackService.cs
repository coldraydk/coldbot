using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LakseBot.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http.Headers;

namespace LakseBot.Services {
    public class SlackService 
    {
        private const string SLACK_URL = "https://slack.com/api/chat.postMessage";
        private static string botToken = System.Environment.GetEnvironmentVariable("BOT_TOKEN"); 

         
        private static HttpClient client = new HttpClient();

        private readonly ILogger<SlackService> logger;
        private readonly IHostingEnvironment env;

        
        public SlackService(ILogger<SlackService> logger, IHostingEnvironment env)
        {
            this.logger = logger;
            this.env = env;
        }

        // public async void SendMessage(string text)
        // {
        //     var parameters = new Payload() 
        //     {
        //         Text = text,
        //     };

        //     var jsonString = JsonConvert.SerializeObject(parameters);
        //     jsonString = jsonString.Replace(@"\\n", @"\n");

        //     logger.LogInformation($"Sending message to server: {jsonString}");

        //     var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

        //     var slackResponse = await client.PostAsync(SLACK_URL, content);
        // }

        public async void SendMessage(string text, string channel)
        {
            var parameters = new Payload() 
            {
                Text = text,
                Token = Uri.EscapeUriString(botToken),
                Channel = channel
            };

            // logger.LogInformation($"Sending message to server: {jsonString}");

            // jsonString = jsonString.Replace(@"\\n", @"\n");

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var data = new NameValueCollection();
            data["payload"] = JsonConvert.SerializeObject(parameters);

            var content = new StringContent(JsonConvert.SerializeObject(data["payload"]), Encoding.UTF8, "application/json");

            logger.LogInformation(await content.ReadAsStringAsync());

            var slackResponse = await client.PostAsync(SLACK_URL, content);

            logger.LogInformation(await slackResponse.Content.ReadAsStringAsync());
        }

        private string channelMapper(string channel) {
            return "";
        }
    }
}