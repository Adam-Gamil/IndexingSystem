using IndexingSystem.Entities;
using IndexingSystem.Repositories;
using IndexingSystem.Services.Filters;
using IndexingSystem.Services.Indexing;

namespace IndexingSystem.Services
{
    public class ContactManagerService
    {
        private readonly IContactRepository _contactRepository;
        private readonly IContactIndexManager _contactIndexManager;

        public ContactManagerService(IContactRepository contactRepository, IContactIndexManager contactIndexManager)
        {
            _contactRepository = contactRepository;
            _contactIndexManager = contactIndexManager;
        }

        public async Task InitializeAsync()
        {
            var contacts = await _contactRepository.LoadAllContactsAsync();
            _contactIndexManager.BuildIndexes(contacts);
        }

        public int GenerateNextId() => _contactIndexManager.GenerateNextId();

        public void AddContact(Contact contact)
        {
            ValidateContact(contact);

            if (_contactIndexManager.EmailExists(contact.Email))
            {
                throw new Exception("A contact with this email already exists.");
            }

            _contactIndexManager.AddContact(contact);
        }

        public bool EditContact(Contact contact)
        {
            ValidateContact(contact);

            var existingWithEmail = _contactIndexManager.GetByEmail(contact.Email);
            if (existingWithEmail != null && existingWithEmail.Id != contact.Id)
            {
                throw new Exception("Another contact already uses this email.");
            }

            return _contactIndexManager.UpdateContact(contact);
        }

        public bool RemoveContact(int contactId)
        {
            return _contactIndexManager.RemoveContact(contactId);
        }

        public Contact? ViewContact(int id)
        {
            return _contactIndexManager.GetById(id);
        }

        public Contact? ViewContact(string email)
        {
            return _contactIndexManager.GetByEmail(email);
        }

        public IEnumerable<Contact> GetAllContacts()
        {
            return _contactIndexManager.GetAllContacts();
        }

        public IEnumerable<Contact> SearchContacts(string prefix)
        {
            return _contactIndexManager.SearchByNamePrefix(prefix);
        }

        public IEnumerable<Contact> FilterContacts(IContactFilter filter)
        {
            var allContacts = _contactIndexManager.GetAllContacts();

            var passedContacts = new List<Contact>();

            foreach (var contact in allContacts)
            {
                if (filter.Apply(contact))
                {
                    passedContacts.Add(contact);
                }
            }

            return passedContacts;
        }

        public async Task SaveChangesAsync()
        {
            var allContacts = _contactIndexManager.GetAllContacts();
            await _contactRepository.SaveContactsAsync(allContacts);
        }

        private void ValidateContact(Contact contact)
        {
            if (string.IsNullOrWhiteSpace(contact.Name))
                throw new Exception("Name cannot be empty.");

            if (string.IsNullOrWhiteSpace(contact.Email))
                throw new Exception("Email cannot be empty.");

            if (!contact.Email.Contains('@') || !contact.Email.Contains('.'))
                throw new Exception("Email format is invalid. Must contain '@' and '.'.");

            if (string.IsNullOrWhiteSpace(contact.Phone))
                throw new Exception("Phone cannot be empty.");
        }
    }
}