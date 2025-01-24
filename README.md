# EonDB

EonDB is a lightweight and flexible database-like framework designed to manage sessions and entities using customizable storage providers. It enables users to store, query, update, and delete objects efficiently, while supporting local storage and cloud storage through pluggable providers like `LocalStorageProvider` and `AzureBlobStorageProvider`.

## Features

- **Session-based Management**: Create and manage sessions to group related entities.
- **CRUD Operations**: Perform Create, Read, Update, and Delete operations on entities.
- **Customizable Storage Providers**: Use pluggable storage providers, including local file system storage or Azure Blob Storage.
- **Serialization**: Automatically serialize and deserialize entities for storage.

---

## Installation

To use EonDB in your project, include the EonDB namespace and implement or use a provided storage provider.
```bash
dotnet add package EonDB
```
```csharp
using EonDB;
```

---

## Getting Started

### Initialization

1. **Choose a storage provider**: Use one of the built-in providers (`LocalStorageProvider`, `AzureBlobStorageProvider`,`AmazonS3StorageProvider`,`DropboxStorageProvider`,`GoogleCloudStorageProvider`) or create your own by implementing the `IStorageProvider` interface.
2. **Initialize EonDB**: Instantiate the `EonDB` class with your chosen provider and optional base path or connection string.

#### Example: Using LocalStorageProvider
```csharp
var localProvider = new LocalStorageProvider();
var database = new EonDB(localProvider, "C:\\EonDB");
```

#### Example: Using AzureBlobStorageProvider
```csharp
var azureProvider = new AzureBlobStorageProvider();
var database = new EonDB(azureProvider, "YourAzureBlobConnectionString");
```

### Managing Sessions

#### Create or Retrieve a Session
```csharp
var session = database.GetOrCreateSession("sessionId");
```

#### Save a Session
```csharp
database.SaveSession(session);
```

### Performing CRUD Operations

#### Add an Entity
Ensure that entities have a property named `Id` to uniquely identify them.
```csharp
session.Add(new MyEntity { Id = "123", Name = "Example" });
```

#### Query Entities
```csharp
var results = session.Query<MyEntity>(e => e.Name.Contains("Example"));
```

#### Update Entities
```csharp
session.Update<MyEntity>(e => e.Id == "123", e => e.Name = "Updated Example");
```

#### Delete Entities
```csharp
session.Delete<MyEntity>(e => e.Id == "123");
```

---

## Storage Providers

### LocalStorageProvider
Stores session data on the local file system.
#### Example
```csharp
var localProvider = new LocalStorageProvider();
var database = new EonDB(localProvider, "C:\\EonDB");
```

### AzureBlobStorageProvider
Stores session data in Azure Blob Storage.
#### Example
```csharp
var azureProvider = new AzureBlobStorageProvider();
var database = new EonDB(azureProvider, "YourAzureBlobConnectionString");
```

---

## Creating Custom Storage Providers

To create a custom storage provider, implement the `IStorageProvider` interface:

```csharp
public interface IStorageProvider
{
    void Initialize(string basePath);
    void SaveFile(string path, byte[] data);
    byte[] ReadFile(string path);
    void DeleteFile(string path);
    void CreateDirectory(string path);
    void DeleteDirectory(string path);
    IEnumerable<string> ListFiles(string path);
    bool FileExists(string path);
    bool DirectoryExists(string path);
}
```

---

## Classes Overview

### EonDB
The main class for managing sessions and performing CRUD operations.
- **Constructor**: Accepts an `IStorageProvider` and an optional base path or connection string.
- **Methods**:
  - `GetOrCreateSession(string sessionId)`
  - `Add<T>(string sessionId, T entity)`
  - `Query<T>(string sessionId, Func<T, bool> predicate)`
  - `Update<T>(string sessionId, Func<T, bool> predicate, Action<T> updateAction)`
  - `Delete<T>(string sessionId, Func<T, bool> predicate)`
  - `SaveSession(Session session)`

### Session
Represents a group of entities.
- **Constructor**: Accepts a session ID and an `EonDB` instance.
- **Methods**:
  - `Add<T>(T entity)`
  - `Query<T>(Func<T, bool> predicate)`
  - `Update<T>(Func<T, bool> predicate, Action<T> updateAction)`
  - `Delete<T>(Func<T, bool> predicate)`
  - `GetCollection<T>()`

### LocalStorageProvider
A built-in implementation of `IStorageProvider` that uses the local file system.

### AzureBlobStorageProvider
A built-in implementation of `IStorageProvider` that uses Azure Blob Storage.

---

## Example Use Case

```csharp
var localProvider = new LocalStorageProvider();
var database = new EonDB(localProvider, "C:\\EonDB");

// Create or get a session
var session = database.GetOrCreateSession("UserSession1");

// Add an entity
session.Add(new MyEntity { Id = "1", Name = "Sample Entity" });

// Query entities
var entities = session.Query<MyEntity>(e => e.Name.Contains("Sample"));

// Update an entity
session.Update<MyEntity>(e => e.Id == "1", e => e.Name = "Updated Entity");

// Delete an entity
session.Delete<MyEntity>(e => e.Id == "1");

// Save the session
database.SaveSession(session);
```

---

## Contributing
Contributions are welcome! Feel free to submit issues or pull requests to enhance the functionality of EonDB.

---

## License
EonDB is licensed under the MIT License.

