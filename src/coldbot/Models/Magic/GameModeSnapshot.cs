using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using ColdBot.Services;

namespace ColdBot.Models.Magic
{
    public class GameModeSnapshot
    {
        public String Id { get; set; }
        public GameMode GameMode { get; set; }
        public List<Deck> Ranking { get; set; }
    }
}