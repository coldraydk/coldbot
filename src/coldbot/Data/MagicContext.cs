using Microsoft.EntityFrameworkCore;
using ColdBot.Models.Magic;

namespace ColdBot.Data
{
    public class MagicContext : DbContext
    {
        public DbSet<Player> Players { get; set; }
        public DbSet<GameResult> MatchResults { get; set; }
        public DbSet<GameMode> GameModes { get; set; }
        public DbSet<GameModeSnapshot> GameModeSnapshots { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Deck> Decks { get; set; }

        public MagicContext(DbContextOptions<MagicContext> options) : base(options)
        {
        }
    }
}