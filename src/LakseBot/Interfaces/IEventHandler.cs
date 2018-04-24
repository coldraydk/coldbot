using LakseBot.Models;

namespace LakseBot.EventHandlers
{
    public interface IEventHandler
    {
        void Handle(Event obj);
    }
}