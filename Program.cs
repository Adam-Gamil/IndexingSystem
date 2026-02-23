using IndexingSystem.Configuration;

namespace IndexingSystem
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string dbPath = AppConfig.GetConnectionString();

            await IndexingSystemApp.RunAsync(dbPath);
        }
    }
}