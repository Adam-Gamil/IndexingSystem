# Modern Contact Indexing System

## 1. System Overview
This is a high-performance, command-line Contact Management System built in **C# / .NET 9.0**. It was engineered to solve the performance bottlenecks associated with traditional linear search operations in contact databases. By implementing a custom in-memory data engine alongside asynchronous JSON storage, the system achieves instant $O(1)$ direct lookups and highly optimized $O(L)$ prefix searches, completely decoupled from the underlying data persistence layer.

---

## 2. System Architecture (UML)

```mermaid
classDiagram
    direction TB

    class Program {
        +Main(args: string[])$ Task
    }

    class IndexingSystemApp {
        +RunAsync(databasePath: string)$ Task
    }

    class AppConfig {
        +GetConnectionString()$ string
    }

    class Contact {
        +Id: int
        +Name: string
        +Email: string
        +Phone: string
        +CreatedAt: DateTime
        +Contact()
        +Contact(id: int, name: string, email: string, phone: string)
    }

    class IContactRepository {
        <<interface>>
        +LoadAllContactsAsync() Task~IEnumerable~Contact~~
        +SaveContactsAsync(contacts: IEnumerable~Contact~) Task
    }

    class JsonContactRepository {
        -_filePath: string
        -_jsonOptions: JsonSerializerOptions
        +JsonContactRepository(filePath: string)
        +LoadAllContactsAsync() Task~IEnumerable~Contact~~
        +SaveContactsAsync(contacts: IEnumerable~Contact~) Task
    }

    class IContactIndexManager {
        <<interface>>
        +GenerateNextId() int
        +BuildIndexes(contacts: IEnumerable~Contact~) void
        +AddContact(contact: Contact) void
        +UpdateContact(updatedContact: Contact) bool
        +RemoveContact(id: int) bool
        +GetById(id: int) Contact?
        +GetByEmail(email: string) Contact?
        +EmailExists(email: string) bool
        +GetAllContacts() IEnumerable~Contact~
        +SearchByNamePrefix(prefix: string) IEnumerable~Contact~
    }

    class ContactIndexManager {
        -_contactsById: Dictionary~int, Contact~
        -_contactsByEmail: Dictionary~string, Contact~
        -_nameIndex: ISearchIndex
        -_random: Random
        +GenerateNextId() int
        +BuildIndexes(contacts: IEnumerable~Contact~) void
        +AddContact(contact: Contact) void
        +UpdateContact(updatedContact: Contact) bool
        +RemoveContact(id: int) bool
        +GetById(id: int) Contact?
        +GetByEmail(email: string) Contact?
        +EmailExists(email: string) bool
        +GetAllContacts() IEnumerable~Contact~
        +SearchByNamePrefix(prefix: string) IEnumerable~Contact~
    }

    class ISearchIndex {
        <<interface>>
        +Insert(key: string, contactId: int) void
        +Remove(key: string, contactId: int) void
        +SearchPrefix(prefix: string) IEnumerable~int~
        +Clear() void
    }

    class NameTrie {
        -_root: NameTrieNode
        +NameTrie()
        +Insert(word: string, contactId: int) void
        +Remove(word: string, contactId: int) void
        +SearchPrefix(prefix: string) IEnumerable~int~
        +Clear() void
        -CollectIdsDfs(node: NameTrieNode, results: HashSet~int~) void
    }

    class NameTrieNode {
        +Children: Dictionary~char, NameTrieNode~
        +ContactIds: HashSet~int~
        +IsLeaf: bool
    }

    class ContactManagerService {
        -_contactRepository: IContactRepository
        -_contactIndexManager: IContactIndexManager
        +ContactManagerService(contactRepository: IContactRepository, contactIndexManager: IContactIndexManager)
        +InitializeAsync() Task
        +GenerateNextId() int
        +AddContact(contact: Contact) void
        +EditContact(contact: Contact) bool
        +RemoveContact(contactId: int) bool
        +ViewContact(id: int) Contact?
        +ViewContact(email: string) Contact?
        +GetAllContacts() IEnumerable~Contact~
        +SearchContacts(prefix: string) IEnumerable~Contact~
        +FilterContacts(filter: IContactFilter) IEnumerable~Contact~
        +SaveChangesAsync() Task
        -ValidateContact(contact: Contact) void
    }

    class IContactFilter {
        <<interface>>
        +Apply(contact: Contact) bool
    }

    class DateCreatedFilter {
        -_targetDate: DateTime
        -_comparisonType: DateComparisonType
        +DateCreatedFilter(targetDate: DateTime, comparisonType: DateComparisonType)
        +Apply(contact: Contact) bool
    }

    class DateComparisonType {
        <<enumeration>>
        Before
        After
        OnExactDate
    }

    class ConsoleInterface {
        -_service: ContactManagerService
        +ConsoleInterface(service: ContactManagerService)
        +StartAsync() Task
        -ShowMenu() void
        -AddContactUI() void
        -EditContactUI() void
        -DeleteContactUI() void
        -ViewContactUI() void
        -ListContactsUI() void
        -SearchUI() void
        -SearchByNameUI() void
        -FilterUI() void
        -FilterByDateUI() void
        -SaveUIAsync() Task
        -FindContactByIdOrEmail() Contact?
        -DisplayContact(contact: Contact, index: int) void
        -WaitForKey() void
    }

    %% Inheritance / Implementation
    JsonContactRepository ..|> IContactRepository
    ContactIndexManager ..|> IContactIndexManager
    NameTrie ..|> ISearchIndex
    DateCreatedFilter ..|> IContactFilter

    %% Dependencies / Associations
    Program ..> IndexingSystemApp
    Program ..> AppConfig
    IndexingSystemApp ..> JsonContactRepository : creates
    IndexingSystemApp ..> ContactIndexManager : creates
    IndexingSystemApp ..> ContactManagerService : creates
    IndexingSystemApp ..> ConsoleInterface : creates

    ContactManagerService --> IContactRepository : _contactRepository
    ContactManagerService --> IContactIndexManager : _contactIndexManager
    ContactManagerService ..> IContactFilter : uses
    ContactManagerService ..> Contact : validates & manages

    ContactIndexManager --> ISearchIndex : _nameIndex
    ContactIndexManager --> "*" Contact : _contactsById

    NameTrie --> NameTrieNode : _root
    NameTrieNode --> "*" NameTrieNode : Children

    ConsoleInterface --> ContactManagerService : _service
    ConsoleInterface ..> Contact : displays
    ConsoleInterface ..> DateCreatedFilter : creates
    ConsoleInterface ..> DateComparisonType : uses

    DateCreatedFilter --> DateComparisonType : _comparisonType
```

---

## 3. Features, Architecture, and Algorithms

This project was built with a strict adherence to Clean Architecture and modern backend engineering practices.

### Algorithms & Data Structures
* **$O(1)$ HashMaps:** Utilizes dual synchronized `Dictionary<TKey, TValue>` structures to provide instant access to contacts by ID or Email.
* **$O(L)$ Prefix Trie (Prefix Tree):** Replaced standard $O(N)$ `IEnumerable.Where` string matching with a custom Trie data structure. Searching for a name prefix (e.g., "Ad") instantly traverses the tree and yields a mapped `HashSet` of IDs without scanning the entire database.

### OOP & SOLID Principles
* **Single Responsibility Principle (SRP):** Complete separation of concerns. `ConsoleInterface` handles UI, `ContactManagerService` handles business routing, `ContactIndexManager` handles memory state, and `JsonContactRepository` handles disk I/O.
* **Open/Closed Principle (OCP) via Strategy Pattern:** Contact filtering uses the `IContactFilter` interface. Complex filters (like `DateCreatedFilter`) can be passed into the service dynamically. New filters can be added to the system without modifying existing service code.
* **Dependency Inversion Principle (DIP):** High-level modules do not depend on low-level modules. `ContactManagerService` depends on abstractions (`IContactRepository`) injected via the constructor, allowing the JSON storage to be easily swapped for a SQL database in the future.

### Advanced Techniques
* **Asynchronous I/O:** Utilizes `async/await` and `System.Text.Json` to prevent thread-blocking during file read/write operations.
* **Optimized Memory Management:** Uses `TryGetValue` to prevent double-hashing bottlenecks, and relies on natural C# Garbage Collection via unreferenced node dropping for safe Trie deletions.

---

## 4. How to Use the Program

### Prerequisites
* [.NET 9.0 SDK](https://dotnet.microsoft.com/download) installed on your machine.

### Installation & Execution
1. Clone the repository to your local machine:
   ```bash
   git clone <YOUR_GITHUB_REPO_URL>
   ```
2. Navigate into the project directory:
   ```bash
   cd IndexingSystem
   ```
3. Run the application:
   ```bash
   dotnet run
   ```
*Note: A pre-populated `contacts_db.json` file is included in the project root. The `.csproj` file is configured to automatically copy this database to your output directory upon building, allowing you to test the $O(1)$ lookups and Trie searches immediately.*

---

## 5. Video Demonstrations

To see the system in action and understand the backend design, please view the following demonstrations:

* 🎥 **[System Overview & Demo (2 Minutes)](INSERT_YOUR_YOUTUBE_LINK_HERE)** *A quick walk-through of the CLI interface, demonstrating the instant Prefix Search and Strategy Pattern Date Filtering.*

* 🎥 **[Technical Breakdown & Architecture (5 Minutes)](INSERT_YOUR_YOUTUBE_LINK_HERE)** *A deep dive into the code. I explain the performance bottlenecks of legacy indexing, how the custom Prefix Trie was built, and how state is safely synchronized across the in-memory indexes.*
