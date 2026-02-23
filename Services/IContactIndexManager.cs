using IndexingSystem.Entities;

namespace IndexingSystem.Services.Indexing
{
    public interface IContactIndexManager
    {
        int GenerateNextId();
        void BuildIndexes(IEnumerable<Contact> contacts);
        void AddContact(Contact contact);
        bool UpdateContact(Contact updatedContact);
        bool RemoveContact(int id);
        Contact? GetById(int id);
        Contact? GetByEmail(string email);
        bool EmailExists(string email);
        IEnumerable<Contact> GetAllContacts();
        IEnumerable<Contact> SearchByNamePrefix(string prefix);
    }
}