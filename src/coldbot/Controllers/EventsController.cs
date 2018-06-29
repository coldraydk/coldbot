using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ColdBot.Models;
using ColdBot.Services;
using ColdBot.Data;

namespace ColdBot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private ILogger<EventsController> logger;
        private readonly MagicService magicLeagueService;

        public EventsController(ILogger<EventsController> logger, SlackService slackService, MagicService magicLeagueService)
        {
            this.logger = logger;
            this.magicLeagueService = magicLeagueService;
        }

        [HttpGet]
        public IActionResult Get(string text)
        {
            if (String.IsNullOrEmpty(text))
                return BadRequest("Please set the text parameter.");

            // coldbot-dev channel.
            var dummyEvent = new Event() { Text = text, User = "U20CAA72L", Channel = "GBEQBFJ5R" };
  
            magicLeagueService.ProcessEvent(dummyEvent);

            return Ok("Done.");
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] SlackRequest request)
        {
            logger.LogInformation(JsonConvert.SerializeObject(request));

            if (!string.IsNullOrEmpty(request?.Challenge))
            {
                return Ok(request.Challenge);
            }

            if (!string.IsNullOrEmpty(request?.Event?.BotId))
            {
                return Ok();
            }

            magicLeagueService.ProcessEvent(request?.Event);

            return Ok();
        }
    }
}
