using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using LakseBot.Services;

namespace LakseBot.Models 
{
    public class MatchResult
    {
        public int ID {get; set;}
        public Player Winner { get; set; }
        public Player Loser { get; set; }
        public int RatingChange { get; set; }
        public DateTime Timestamp { get; set; }
    }
}