using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LakseBot.Services;

namespace LakseBot.Models 
{
    public class League
    {
        public int ID {get; set;}
        public String Name { get; set; }
        public DateTime StartTime { get; set; }
    }
}