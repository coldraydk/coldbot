using LakseBot.EventHandlers;
using LakseBot.Models;
using LakseBot.Services;

namespace LakseBot.EventHandlers
 {
    public class GenericResponse : IEventHandler
    {
        private SlackService slackService;

        public GenericResponse(SlackService slackService)
        {
            this.slackService = slackService;
        }
        
        public void Handle(Event obj)
        {
            
        }
    }
}