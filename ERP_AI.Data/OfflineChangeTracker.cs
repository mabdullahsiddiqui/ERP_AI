using System;
using System.Collections.Generic;

namespace ERP_AI.Data
{
    public enum ChangeType { Add, Update, Delete }

    public class OfflineChange
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string EntityType { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public ChangeType ChangeType { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? DataJson { get; set; }
    }

    public class OfflineChangeTracker
    {
        public List<OfflineChange> Changes { get; set; } = new();

        public void TrackChange(string entityType, Guid entityId, ChangeType changeType, string? dataJson = null)
        {
            Changes.Add(new OfflineChange
            {
                EntityType = entityType,
                EntityId = entityId,
                ChangeType = changeType,
                DataJson = dataJson
            });
        }

        public void Clear() => Changes.Clear();
    }
}
