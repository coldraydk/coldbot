using Microsoft.EntityFrameworkCore;
using ColdBot.Models.Magic;

namespace ColdBot.Data
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