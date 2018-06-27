namespace ColdBot.Data
{
    public static class DbInitializer
    {
        public static void Initialize(MagicContext context)
        {
            context.Database.EnsureCreated();

            context.SaveChanges();
        }
    }
}