using Microsoft.EntityFrameworkCore;
using LakseBot.Models;

namespace LakseBot.Data
{
    public class MagicLeagueContext : DbContext
    {
        public MagicLeagueContext(DbContextOptions<MagicLeagueContext> options) : base(options)
        {
        }
        
        public DbSet<Player> Players { get; set; }
        public DbSet<MatchResult> MatchResults { get;set; }
    }
}