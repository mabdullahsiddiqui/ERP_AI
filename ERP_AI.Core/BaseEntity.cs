using System;

namespace ERP_AI.Core
{
    public abstract class BaseEntity
    {
        [System.ComponentModel.DataAnnotations.Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [System.ComponentModel.DataAnnotations.Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [System.ComponentModel.DataAnnotations.Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; }
        public DateTime? LastSyncedAt { get; set; }
        public SyncStatus SyncStatus { get; set; }
        [System.ComponentModel.DataAnnotations.Timestamp]
        public byte[]? RowVersion { get; set; } // For optimistic concurrency
        public string? UpdatedBy { get; set; } // Audit trail: user tracking
    }

    public enum SyncStatus
    {
        NotSynced,
        Synced,
        Pending,
        Conflict
    }
}
