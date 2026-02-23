using IndexingSystem.Entities;
using IndexingSystem.Services;
using IndexingSystem.Services.Filters;

namespace IndexingSystem.Presentation
{
    public class ConsoleInterface
    {
        private readonly ContactManagerService _service;

        public ConsoleInterface(ContactManagerService service)
        {
            _service = service;
        }

        public async Task StartAsync()
        {
            Console.WriteLine("Initializing System...");

            await _service.InitializeAsync();

            var contacts = _service.GetAllContacts().ToList();

            if (contacts.Any())
            {
                Console.WriteLine($"Successfully loaded {contacts.Count} contacts from the database.");
            }
            else
            {
                Console.WriteLine("No contacts found in the database. Starting fresh.");
            }

            WaitForKey();

            bool isRunning = true;
            while (isRunning)
            {
                Console.Clear();
                ShowMenu();
                Console.Write("\nEnter your choice (1-9): ");
                string? choice = Console.ReadLine();

                Console.WriteLine();

                switch (choice)
                {
                    case "1": AddContactUI(); break;
                    case "2": EditContactUI(); break;
                    case "3": DeleteContactUI(); break;
                    case "4": ViewContactUI(); break;
                    case "5": ListContactsUI(); break;
                    case "6": SearchUI(); break;
                    case "7": FilterUI(); break;
                    case "8": await SaveUIAsync(); break;
                    case "9":
                        isRunning = false;
                        Console.WriteLine("Exiting application");
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please enter a number between 1 and 9.");
                        WaitForKey();
                        break;
                }
            }
        }

        private void ShowMenu()
        {
            Console.WriteLine("--- Contact Manager ---");
            Console.WriteLine("1. Add Contact");
            Console.WriteLine("2. Edit Contact");
            Console.WriteLine("3. Delete Contact");
            Console.WriteLine("4. View Contact");
            Console.WriteLine("5. List Contacts");
            Console.WriteLine("6. Search");
            Console.WriteLine("7. Filter");
            Console.WriteLine("8. Save");
            Console.WriteLine("9. Exit");
        }

        private void AddContactUI()
        {
            try
            {
                int id = _service.GenerateNextId();

                Console.Write("Enter Name: ");
                string name = Console.ReadLine() ?? string.Empty;

                Console.Write("Enter Email: ");
                string email = Console.ReadLine() ?? string.Empty;

                Console.Write("Enter Phone: ");
                string phone = Console.ReadLine() ?? string.Empty;

                var newContact = new Contact(id, name, email, phone);

                _service.AddContact(newContact);
                Console.WriteLine($"\nContact added successfully with ID {id} (in-memory). Remember to Save!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
            }

            WaitForKey();
        }

        private void EditContactUI()
        {
            var existingContact = FindContactByIdOrEmail();

            if (existingContact == null)
            {
                WaitForKey();
                return;
            }

            Console.WriteLine("\nCurrent contact details:");
            DisplayContact(existingContact);

            Console.WriteLine("Leave a field empty and press Enter to keep the current value.\n");

            Console.Write($"  Name ({existingContact.Name}): ");
            string nameInput = Console.ReadLine() ?? string.Empty;
            string name = string.IsNullOrWhiteSpace(nameInput) ? existingContact.Name : nameInput;

            Console.Write($"  Email ({existingContact.Email}): ");
            string emailInput = Console.ReadLine() ?? string.Empty;
            string email = string.IsNullOrWhiteSpace(emailInput) ? existingContact.Email : emailInput;

            Console.Write($"  Phone ({existingContact.Phone}): ");
            string phoneInput = Console.ReadLine() ?? string.Empty;
            string phone = string.IsNullOrWhiteSpace(phoneInput) ? existingContact.Phone : phoneInput;

            try
            {
                var updatedContact = new Contact { Id = existingContact.Id, Name = name, Email = email, Phone = phone };

                _service.EditContact(updatedContact);
                Console.WriteLine("\nContact updated successfully (in-memory). Remember to Save!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
            }

            WaitForKey();
        }

        private void DeleteContactUI()
        {
            Console.Write("Enter Contact ID to delete: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var contact = _service.ViewContact(id);
                if (contact == null)
                {
                    Console.WriteLine("Contact not found.");
                    WaitForKey();
                    return;
                }

                Console.WriteLine("\nYou are about to delete:");
                DisplayContact(contact);

                Console.Write("Are you sure? (y/n): ");
                string? confirm = Console.ReadLine()?.Trim().ToLower();

                if (confirm == "y")
                {
                    _service.RemoveContact(id);
                    Console.WriteLine("Contact deleted successfully (in-memory). Remember to Save!");
                }
                else
                {
                    Console.WriteLine("Delete canceled.");
                }
            }
            else
            {
                Console.WriteLine("Invalid ID format.");
            }

            WaitForKey();
        }

        private void ViewContactUI()
        {
            var contact = FindContactByIdOrEmail();

            if (contact != null)
            {
                Console.WriteLine();
                DisplayContact(contact);
            }

            WaitForKey();
        }

        private void ListContactsUI()
        {
            var contacts = _service.GetAllContacts().ToList();

            if (contacts.Any())
            {
                Console.WriteLine($"--- All Contacts ({contacts.Count}) ---\n");
                int index = 1;
                foreach (var contact in contacts)
                {
                    DisplayContact(contact, index);
                    index++;
                }
            }
            else
            {
                Console.WriteLine("No contacts found. Try adding some!");
            }

            WaitForKey();
        }

        private void SearchUI()
        {
            Console.WriteLine("--- Search Menu ---");
            Console.WriteLine("1. Search by Name Prefix");
            Console.Write("\nEnter your choice: ");
            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    SearchByNameUI();
                    break;
                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }

            WaitForKey();
        }

        private void SearchByNameUI()
        {
            Console.Write("Enter Name Prefix to search: ");
            string prefix = Console.ReadLine() ?? string.Empty;

            var results = _service.SearchContacts(prefix).ToList();

            if (results.Any())
            {
                Console.WriteLine($"\nFound {results.Count} match(es):\n");
                int index = 1;
                foreach (var contact in results)
                {
                    DisplayContact(contact, index);
                    index++;
                }
            }
            else
            {
                Console.WriteLine("No contacts found matching that prefix.");
            }
        }

        private void FilterUI()
        {
            Console.WriteLine("--- Filter Menu ---");
            Console.WriteLine("1. Filter by Creation Date");
            Console.Write("\nEnter your choice: ");
            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    FilterByDateUI();
                    break;
                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }

            WaitForKey();
        }

        private void FilterByDateUI()
        {
            Console.Write("Enter Date (yyyy-MM-dd): ");

            if (DateTime.TryParse(Console.ReadLine(), out DateTime targetDate))
            {
                Console.WriteLine("Select comparison type:");
                Console.WriteLine("1. Before this date");
                Console.WriteLine("2. After this date");
                Console.WriteLine("3. On this exact date");
                Console.Write("Choice (1-3): ");

                string? compChoice = Console.ReadLine();
                DateComparisonType compType;

                switch (compChoice)
                {
                    case "1": compType = DateComparisonType.Before; break;
                    case "2": compType = DateComparisonType.After; break;
                    case "3": compType = DateComparisonType.OnExactDate; break;
                    default:
                        Console.WriteLine("Invalid choice. Canceling filter.");
                        return;
                }

                var filter = new DateCreatedFilter(targetDate, compType);
                var results = _service.FilterContacts(filter).ToList();

                if (results.Any())
                {
                    Console.WriteLine($"\nFound {results.Count} matching contact(s):\n");
                    int index = 1;
                    foreach (var contact in results)
                    {
                        DisplayContact(contact, index);
                        index++;
                    }
                }
                else
                {
                    Console.WriteLine("No contacts found matching that filter.");
                }
            }
            else
            {
                Console.WriteLine("Invalid date format. Please use a format like 2026-02-24.");
            }
        }

        private async Task SaveUIAsync()
        {
            Console.WriteLine("Saving to JSON database...");
            await _service.SaveChangesAsync();
            Console.WriteLine("Save complete!");

            WaitForKey();
        }


        private Contact? FindContactByIdOrEmail()
        {
            Console.Write("Enter Contact ID or Email: ");
            string? input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("No input provided.");
                return null;
            }

            if (int.TryParse(input, out int id))
            {
                var contact = _service.ViewContact(id);
                if (contact == null)
                    Console.WriteLine("No contact found with that ID.");
                return contact;
            }
            else
            {
                var contact = _service.ViewContact(input);
                if (contact == null)
                    Console.WriteLine("No contact found with that email.");
                return contact;
            }
        }

        private void DisplayContact(Contact contact, int index = 0)
        {
            if (index > 0)
            {
                Console.WriteLine($"Contact {index}:\n");
            }

            Console.WriteLine($"  ID:         {contact.Id}");
            Console.WriteLine($"  Name:       {contact.Name}");
            Console.WriteLine($"  Email:      {contact.Email}");
            Console.WriteLine($"  Phone:      {contact.Phone}");
            Console.WriteLine($"  Created At: {contact.CreatedAt:yyyy-MM-dd}");
            Console.WriteLine();
        }

        private void WaitForKey()
        {
            Console.WriteLine("\nPress any key to return to the main menu...");
            Console.ReadKey(true);
        }
    }
}
