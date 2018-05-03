using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LakseBot.Services;

namespace LakseBot.Models 
{
    public class MagicLeague
    {
        private List<Player> players;
        private List<MatchResult> matches;

        /* 
        https://en.wikipedia.org/wiki/Elo_rating_system. 
        
        Chess players use KFACTOR 16 for masters, 24 for high tier people and 32 for people below 2100.
        Divisor is constant across all skill levels.
        */
        private const int ELO_KFACTOR = 32;
        private const int ELO_DIVISOR = 400;

        public string Name { get; set; }

        public enum Winner
        {
            Player1,
            Player2
        }

        public MagicLeague(string name)
        {
            this.players = new List<Player>();
            this.matches = new List<MatchResult>();

            this.Name = name;
        }

        public void AddPlayer(Player player)
        {
            players.Add(player);
        }

        public List<Player> GetPlayers()
        {
            return players;
        }

        

       

        

        public List<MatchResult> GetMatches()
        {
            return matches;
        }
    }
}