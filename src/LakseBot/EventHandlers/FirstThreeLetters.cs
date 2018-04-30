using System;
using System.Linq;
using LakseBot.EventHandlers;
using LakseBot.Models;
using LakseBot.Services;

namespace LakseBot.EventHandlers
 {
    public class FirstThreeLetters
    {
        public static void Handle(object sender, Event obj)
        {
            Console.WriteLine(obj.Text.Substring(0,3));
        }
    }
}