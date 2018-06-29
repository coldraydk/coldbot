using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColdBot.Models.Magic
{
    public class Deck
    {
        public String Id { get; set; }
        public String DeckName { get; set; }
        public Player Player { get; set; }

        public Deck()
        {

        }

        public Deck(String name, Player player)
        {
            DeckName = name;
            Player = player;
        }
    }
}