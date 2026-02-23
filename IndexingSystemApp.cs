using IndexingSystem.Presentation;
using IndexingSystem.Repositories;
using IndexingSystem.Services;
using IndexingSystem.Services.Indexing;

namespace IndexingSystem
{
    public static class IndexingSystemApp
    {
        public static async Task RunAsync(string databasePath)
        {
            IContactRepository repository = new JsonContactRepository(databasePath);
            IContactIndexManager indexManager = new ContactIndexManager();
            ContactManagerService service = new ContactManagerService(repository, indexManager);

            ConsoleInterface ui = new ConsoleInterface(service);

            await ui.StartAsync();
        }
    }
}