using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ColdBot.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http.Headers;

namespace ColdBot.Services
{
    public class SlackService
    {
        private static string BOT_TOKEN = System.Environment.GetEnvironmentVariable("BOT_TOKEN");
        static readonly String SLACK_URL = "https://slack.com/api/chat.postMessage";

        private static HttpClient client = new HttpClient();

        private readonly ILogger<SlackService> logger;
        private readonly IHostingEnvironment env;

        public SlackService(ILogger<SlackService> logger, IHostingEnvironment env)
        {
            this.logger = logger;
            this.env = env;

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", BOT_TOKEN);
        }

        public async void SendMessage(string text, string channel)
        {
            var payload = new Payload()
            {
                Text = text,
                Channel = channel
            };
            
            var jsonString = JsonConvert.SerializeObject(payload);

            logger.LogInformation($"Sending message to server: {jsonString}");
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            var slackResponse = await client.PostAsync(SLACK_URL, content);
        }
    }
}