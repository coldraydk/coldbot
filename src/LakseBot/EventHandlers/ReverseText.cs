using System;
using LakseBot.EventHandlers;
using LakseBot.Models;
using LakseBot.Services;

namespace LakseBot.EventHandlers
 {
    public class ReverseText
    {
        public static void Handle(object sender, Event obj)
        {
            var text = obj.Text;
            var charArray = text.ToCharArray();
            Array.Reverse(charArray);

            Console.WriteLine(new string(charArray));
        }
    }
}