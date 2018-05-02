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

namespace LakseBot.Services {
    public class SlackService 
    {
        private const string SLACK_URL = "";

         
        private static HttpClient client = new HttpClient();

        private readonly ILogger<SlackService> logger;
        private readonly IHostingEnvironment env;

        
        public SlackService(ILogger<SlackService> logger, IHostingEnvironment env)
        {
            this.logger = logger;
            this.env = env;
        }

        public async void SendMessage(string text)
        {
            var parameters = new Payload() 
            {
                Text = text,
            };

            var jsonString = JsonConvert.SerializeObject(parameters);
            jsonString = jsonString.Replace(@"\\n", @"\n");

            logger.LogInformation($"Sending message to server: {jsonString}");

            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            var slackResponse = await client.PostAsync(SLACK_URL, content);
        }
    }
}