using System;

namespace ERP_AI.Core
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; }
        public DateTime? LastSyncedAt { get; set; }
        public SyncStatus SyncStatus { get; set; }
    }

    public enum SyncStatus
    {
        NotSynced,
        Synced,
        Pending,
        Conflict
    }
}
