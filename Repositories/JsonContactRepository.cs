using System.Text.Json;
using IndexingSystem.Entities;

namespace IndexingSystem.Repositories
{
    public class JsonContactRepository : IContactRepository
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _jsonOptions;

        public JsonContactRepository(string filePath)
        {
            _filePath = filePath;

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<IEnumerable<Contact>> LoadAllContactsAsync()
        {
            if (!File.Exists(_filePath))
            {
                return Enumerable.Empty<Contact>();
            }

            using FileStream stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            var contacts = await JsonSerializer.DeserializeAsync<IEnumerable<Contact>>(stream, _jsonOptions);

            return contacts ?? Enumerable.Empty<Contact>();
        }

        public async Task SaveContactsAsync(IEnumerable<Contact> contacts)
        {
            using FileStream stream = new FileStream(_filePath, FileMode.Create, FileAccess.Write, FileShare.None);

            await JsonSerializer.SerializeAsync(stream, contacts, _jsonOptions);
        }
    }
}