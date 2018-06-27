using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using ColdBot.Services;

namespace ColdBot.Models.Magic
{
    public class MatchResult
    {
        public int Id { get; set; }
        public GameMode GameMode { get; set; }
        public List<Player> Winners { get; set; }
        public List<Player> Losers { get; set; }
        public DateTime Timestamp { get; set; }
    }
}