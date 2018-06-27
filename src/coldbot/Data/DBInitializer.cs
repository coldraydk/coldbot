namespace LakseBot.Data
{
    public static class DbInitializer
    {
        public static void Initialize(MagicLeagueContext context)
        {
            context.Database.EnsureCreated();

            context.SaveChanges();
        }
    }
}