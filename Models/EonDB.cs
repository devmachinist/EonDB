using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EonDB
{
    public class EonDB
    {
        private readonly IStorageProvider _storageProvider;

        public EonDB(IStorageProvider storageProvider, string? Base = null)
        {
            _storageProvider = storageProvider;
            if (Base != null)
            {
                _storageProvider.Initialize(Base); // Initialize with basePath or connection string if needed
            }
        }

        public Session GetOrCreateSession(string sessionId)
        {
            var sessionPath = GetSessionPath(sessionId);

            if (_storageProvider.DirectoryExists(sessionPath))
            {
                // Session exists; return it
                return GetSession(sessionId);
            }

            // Create a new session if it doesn't exist
            _storageProvider.CreateDirectory(sessionPath);
            return new Session(sessionId, this);
        }

        public void Add<T>(string sessionId, T entity)
        {
            var entityType = typeof(T).Name;
            var entityId = GetEntityId(entity);
            var filePath = GetEntityFilePath(sessionId, entityType, entityId);

            var serializedData = SerializeEntity(entity);
            _storageProvider.SaveFile(filePath, serializedData);
        }

        public List<T> Query<T>(string sessionId, Func<T, bool> predicate)
        {
            var entityType = typeof(T).Name;
            var folderPath = GetEntityFolderPath(sessionId, entityType);

            if (!_storageProvider.DirectoryExists(folderPath))
                return new List<T>();

            return _storageProvider.ListFiles(folderPath)
                .Select(file => DeserializeEntity<T>(_storageProvider.ReadFile(file)))
                .Where(predicate)
                .ToList();
        }

        public void Update<T>(string sessionId, Func<T, bool> predicate, Action<T> updateAction)
        {
            var entityType = typeof(T).Name;
            var folderPath = GetEntityFolderPath(sessionId, entityType);

            if (!_storageProvider.DirectoryExists(folderPath))
                return;

            foreach (var file in _storageProvider.ListFiles(folderPath))
            {
                var entity = DeserializeEntity<T>(_storageProvider.ReadFile(file));
                if (predicate(entity))
                {
                    updateAction(entity);
                    _storageProvider.SaveFile(file, SerializeEntity(entity));
                }
            }
        }

        public void Delete<T>(string sessionId, Func<T, bool> predicate)
        {
            var entityType = typeof(T).Name;
            var folderPath = GetEntityFolderPath(sessionId, entityType);

            if (!_storageProvider.DirectoryExists(folderPath))
                return;

            foreach (var file in _storageProvider.ListFiles(folderPath))
            {
                var entity = DeserializeEntity<T>(_storageProvider.ReadFile(file));
                if (predicate(entity))
                {
                    _storageProvider.DeleteFile(file);
                }
            }
        }

        public Session GetSession(string sessionId)
        {
            if (!_storageProvider.DirectoryExists(GetSessionPath(sessionId)))
                throw new DirectoryNotFoundException($"Session {sessionId} does not exist.");

            var session = new Session(sessionId, this);

            foreach (var entityTypeFolder in _storageProvider.ListFiles(GetSessionPath(sessionId)))
            {
                var entityType = Path.GetFileName(entityTypeFolder);
                var entities = _storageProvider.ListFiles(entityTypeFolder)
                    .Select(file => DeserializeEntity<object>(_storageProvider.ReadFile(file)))
                    .ToList();
                session.AddCollection($"{entityType}List", entities);
            }

            return session;
        }

        public void SaveSession(Session session)
        {
            var sessionPath = GetSessionPath(session.Id);

            if (_storageProvider.DirectoryExists(sessionPath))
                _storageProvider.DeleteDirectory(sessionPath);

            _storageProvider.CreateDirectory(sessionPath);

            foreach (var kvp in session.GetAllCollections())
            {
                var entityType = kvp.Key.Replace("List", "");
                var folderPath = GetEntityFolderPath(session.Id, entityType);

                _storageProvider.CreateDirectory(folderPath);

                foreach (var entity in (IEnumerable<object>)kvp.Value)
                {
                    var entityId = GetEntityId(entity);
                    var filePath = Path.Combine(folderPath, $"{entityId}.bin");
                    _storageProvider.SaveFile(filePath, SerializeEntity(entity));
                }
            }
        }

        // Helper Methods
        private string GetSessionPath(string sessionId) => Path.Combine("Sessions", sessionId);

        private string GetEntityFolderPath(string sessionId, string entityType)
            => Path.Combine(GetSessionPath(sessionId), entityType);

        private string GetEntityFilePath(string sessionId, string entityType, string entityId)
            => Path.Combine(GetEntityFolderPath(sessionId, entityType), $"{entityId}.bin");

        private byte[] SerializeEntity(object entity)
        {
            using var ms = new MemoryStream();
            var formatter = new BinaryFormatter();
            formatter.Serialize(ms, entity);
            return ms.ToArray();
        }

        private T DeserializeEntity<T>(byte[] data)
        {
            using var ms = new MemoryStream(data);
            var formatter = new BinaryFormatter();
            return (T)formatter.Deserialize<T>(ms);
        }

        private string GetEntityId(object entity)
        {
            var idProperty = entity.GetType().GetProperty("Id");
            if (idProperty == null)
                throw new InvalidOperationException($"Entity type {entity.GetType().Name} does not have an Id property.");
            return idProperty.GetValue(entity)?.ToString();
        }
    }
}
