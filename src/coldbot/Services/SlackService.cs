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
        private static string BOT_THIES_WEBHOOK = System.Environment.GetEnvironmentVariable("BOT_THIES_WEBHOOK"); 
        private static string BOT_LAKSEBOT_WEBHOOK = System.Environment.GetEnvironmentVariable("BOT_LAKSEBOT_WEBHOOK"); 

        private static HttpClient client = new HttpClient();

        private readonly ILogger<SlackService> logger;
        private readonly IHostingEnvironment env;
        
        public SlackService(ILogger<SlackService> logger, IHostingEnvironment env)
        {
            this.logger = logger;
            this.env = env;
        }

        public async void SendMessage(string text, string channel)
        {
            string slackUrl = channelMapper(channel);

            if (String.IsNullOrEmpty(slackUrl))
                return;

            var payload = new Payload() 
            {
                Text = text
            };

            var jsonString = JsonConvert.SerializeObject(payload);

            logger.LogInformation($"Sending message to server: {jsonString}");

            jsonString = jsonString.Replace(@"\\n", @"\n");

            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            var slackResponse = await client.PostAsync(slackUrl, content);

            logger.LogInformation(await slackResponse.Content.ReadAsStringAsync());
        }

        private string channelMapper(string channel) {
            if (channel.ToLower().Equals("GAC3TKTV5".ToLower()))
                return BOT_LAKSEBOT_WEBHOOK;
            
            logger.LogInformation("I do not have a webhook for that channel, but I did carry out the command.");
            
            return String.Empty;
        }
    }
}