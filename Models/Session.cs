using System;
using System.Collections.Generic;
using System.Linq;

namespace EonDB
{
    public class Session
    {
        public string Id { get; }
        private readonly EonDB _database;
        private readonly Dictionary<string, object> _collections = new();

        public Session(string id, EonDB database)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public void Add<T>(T entity)
        {
            _database.Add(Id, entity);
            GetOrCreateCollection<T>().Add(entity);
        }

        public List<T> Query<T>(Func<T, bool> predicate)
        {
            var results = _database.Query(Id, predicate);
            var collection = GetOrCreateCollection<T>();
            foreach (var entity in results.Where(e => !collection.Contains(e)))
            {
                collection.Add(entity);
            }
            return results;
        }

        public void Update<T>(Func<T, bool> predicate, Action<T> updateAction)
        {
            _database.Update(Id, predicate, updateAction);
            var collection = GetOrCreateCollection<T>();
            foreach (var entity in collection.Where(predicate))
            {
                updateAction(entity);
            }
        }

        public void Delete<T>(Func<T, bool> predicate)
        {
            _database.Delete(Id, predicate);
            var collection = GetOrCreateCollection<T>();
            var itemsToRemove = collection.Where(predicate).ToList();
            foreach (var item in itemsToRemove)
            {
                collection.Remove(item);
            }
        }
        public void AddCollection<T>(string name, List<T> list)
        {
            _collections.Add(name, list);
        }

        public List<T> GetCollection<T>()
        {
            return GetOrCreateCollection<T>();
        }

        private List<T> GetOrCreateCollection<T>()
        {
            var typeName = typeof(T).Name + "List";
            if (!_collections.ContainsKey(typeName))
            {
                var collection = _database.Query<T>(Id, _ => true);
                _collections[typeName] = new List<T>(collection);
            }

            return (List<T>)_collections[typeName];
        }
        public Dictionary<string, object> GetAllCollections()
        {
            return _collections;
        }

        internal void AddCollection(string v, List<object> entities)
        {
            throw new NotImplementedException();
        }
    }
}
