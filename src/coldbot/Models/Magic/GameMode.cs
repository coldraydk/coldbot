using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using ColdBot.Services;

namespace ColdBot.Models.Magic
{
    public class GameMode
    {
        public int Id { get; set; }
        public String ShortName { get; set; }
        public String Name { get; set; }
    }
}