using System;
using System.Collections.Generic;

namespace ColdBot.Models {
    public class Command
    {
        public String Name { get; set; }
        public List<String> Parameters { get; set; }

        public Command()
        {
            this.Parameters = new List<String>();
        }

        public static Command Parse(string input)
        {
            if (input == null)
                return null;
            
            var splittedInput = input.Split(" ");

            if (!splittedInput[0].ToLower().Equals("magic") || splittedInput.Length < 2)
                return null;

            Command command = new Command();

            command.Name = splittedInput[1].ToLower();

            for (int i = 2; i<splittedInput.Length; i++)
                command.Parameters.Add(splittedInput[i]);

            
            return command;
        }
    }
}