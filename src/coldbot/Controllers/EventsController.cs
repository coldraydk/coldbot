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
using LakseBot.Data;

namespace LakseBot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private ILogger<EventsController> logger;
        private readonly MagicLeagueService magicLeagueService;

        public EventsController(ILogger<EventsController> logger, SlackService slackService, MagicLeagueService magicLeagueService)
        {
            this.logger = logger;
            this.magicLeagueService = magicLeagueService;
        }

        [HttpGet]
        public IActionResult Get(string text)
        {
            if (String.IsNullOrEmpty(text))
                return BadRequest("Please define a message.");

            var dummyEvent = new Event() { Text = text, User = "U20CAA72L", Channel = "GA0Q1SLGK" };
  
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
