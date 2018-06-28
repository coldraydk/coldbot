using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColdBot.Models.Magic
{
    public class Player
    {
        [Key]
        public string Name { get; set; }    

        public override bool Equals(Object obj) 
        {
            return Name == ((Player)obj).Name;
        }
    }
}