using System;

namespace EonDB
{
    [Serializable]
    public class MetadataEntry
    {
        public string SessionId { get; set; }
        public string EntityId { get; set; }
        public string EntityType { get; set; }

        public MetadataEntry(string sessionId, string entityId, string entityType)
        {
            SessionId = sessionId;
            EntityId = entityId;
            EntityType = entityType;
        }
    }
}
