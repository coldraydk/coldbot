using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using LakseBot.Models;
using LakseBot.Services;
using LakseBot.EventHandlers;

namespace LakseBot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private ILogger<EventsController> logger;
        public event EventHandler<Event> onMessageReceived;

        public EventsController(ILogger<EventsController> logger, SlackService slackService)
        {
            this.logger = logger;

            this.onMessageReceived += ReverseText.Handle;
            this.onMessageReceived += FirstThreeLetters.Handle;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var testEvent = new Event() { Text = "Some text message"};
            
            onMessageReceived?.Invoke(null, testEvent);

            return Ok("Hi guys");
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] SlackRequest request)
        {
            logger.LogInformation(JsonConvert.SerializeObject(request));

            if (!string.IsNullOrEmpty(request.Challenge))
            {
                return Ok(request.Challenge);
            }

            if (!string.IsNullOrEmpty(request.Event.BotId))
            {
                return Ok();
            }

            onMessageReceived?.Invoke(null, request.Event);

            return Ok();
        }
    }
}
