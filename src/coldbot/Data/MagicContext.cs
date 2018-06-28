using Microsoft.EntityFrameworkCore;
using ColdBot.Models.Magic;

namespace ColdBot.Data
{
    public class MagicContext : DbContext
    {
        public MagicContext(DbContextOptions<MagicContext> options) : base(options)
        {
        }

        public DbSet<Player> Players { get; set; }
        public DbSet<MatchResult> MatchResults { get;set; }
    }
}