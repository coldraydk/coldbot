using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LakseBot.Models
{
    public class Player
    {
        [Key]
        public string Name { get; set; }    
        public int Rating { get; set; }

        public override bool Equals(Object obj) 
        {
            return Name == ((Player)obj).Name;
        }
    }
}