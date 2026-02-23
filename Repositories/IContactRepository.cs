using IndexingSystem.Entities;

namespace IndexingSystem.Repositories
{
    public interface IContactRepository
    {
        Task SaveContactsAsync(IEnumerable<Contact> contacts);

        Task<IEnumerable<Contact>> LoadAllContactsAsync();
    }
}