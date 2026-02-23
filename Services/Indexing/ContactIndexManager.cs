using System;
using System.Collections.Generic;
using IndexingSystem.Entities;

namespace IndexingSystem.Services.Indexing
{
    public class ContactIndexManager : IContactIndexManager
    {
        private readonly Dictionary<int, Contact> _contactsById = new Dictionary<int, Contact>();
        private readonly Dictionary<string, Contact> _contactsByEmail = new Dictionary<string, Contact>();
        private readonly ISearchIndex _nameIndex = new NameTrie();
        private readonly Random _random = new Random();

        public int GenerateNextId()
        {
            int id;
            do
            {
                id = _random.Next(1, 1_000_001);
            } while (_contactsById.ContainsKey(id));

            return id;
        }

        public void BuildIndexes(IEnumerable<Contact> contacts)
        {
            _contactsById.Clear();
            _contactsByEmail.Clear();
            _nameIndex.Clear();

            foreach (var contact in contacts)
            {
                AddContact(contact);
            }
        }

        public void AddContact(Contact contact)
        {
            _contactsById[contact.Id] = contact;
            _contactsByEmail[contact.Email] = contact;

            _nameIndex.Insert(contact.Name, contact.Id);
        }

        public bool UpdateContact(Contact updatedContact)
        {
            if (!_contactsById.TryGetValue(updatedContact.Id, out var existingContact))
            {
                return false;
            }

            _contactsByEmail.Remove(existingContact.Email);
            _nameIndex.Remove(existingContact.Name, existingContact.Id);

            existingContact.Name = updatedContact.Name;
            existingContact.Email = updatedContact.Email;
            existingContact.Phone = updatedContact.Phone;

            _contactsByEmail[existingContact.Email] = existingContact;
            _nameIndex.Insert(existingContact.Name, existingContact.Id);

            return true;
        }

        public bool RemoveContact(int id)
        {
            if (!_contactsById.TryGetValue(id, out var contactToRemove))
            {
                return false;
            }

            _contactsByEmail.Remove(contactToRemove.Email);
            _nameIndex.Remove(contactToRemove.Name, id);
            _contactsById.Remove(id);

            return true;
        }

        public Contact? GetById(int id)
        {
            _contactsById.TryGetValue(id, out var contact);
            return contact;
        }

        public Contact? GetByEmail(string email)
        {
            _contactsByEmail.TryGetValue(email, out var contact);
            return contact;
        }

        public bool EmailExists(string email)
        {
            return _contactsByEmail.ContainsKey(email);
        }

        public IEnumerable<Contact> GetAllContacts()
        {
            return _contactsById.Values;
        }

        public IEnumerable<Contact> SearchByNamePrefix(string prefix)
        {
            var ids = _nameIndex.SearchPrefix(prefix);
            var results = new List<Contact>();

            foreach (var id in ids)
            {
                if (_contactsById.TryGetValue(id, out var contact))
                {
                    results.Add(contact);
                }
            }

            return results;
        }
    }
}