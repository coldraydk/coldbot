using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColdBot.Models.Magic
{
    public class Rating
    {
        public int Id { get; set; }
        public GameMode GameMode { get; set; }
        public Player Player { get; set; }

        public double Mean { get; set; }
        public double StandardDeviation { get; set; }
        public double ConservativeRating { get; set; }
    }
}