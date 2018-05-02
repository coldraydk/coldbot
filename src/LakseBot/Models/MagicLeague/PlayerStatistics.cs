using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LakseBot.Services;

namespace LakseBot.Models 
{
    public class PlayerStatistics
    {
        public Player Player { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public List<MatchResult> MatchHistory { get; set; }
    }
}