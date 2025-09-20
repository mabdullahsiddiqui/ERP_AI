using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ERP_AI.Core;

namespace ERP_AI.Data
{
    // Phase 3: Advanced Sync Data Models
    
    /// <summary>
    /// Container for sync data packages sent between client and server
    /// </summary>
    public class SyncPackage
    {
        public Guid PackageId { get; set; } = Guid.NewGuid();
        public string CompanyId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Version { get; set; } = "1.0";
        public List<ChangeSet> ChangeSets { get; set; } = new();
        public SyncMetadata Metadata { get; set; } = new();
        public string Checksum { get; set; } = string.Empty;
    }

    /// <summary>
    /// Collection of changes for a specific entity type
    /// </summary>
    public class ChangeSet
    {
        public string EntityType { get; set; } = string.Empty;
        public List<SyncEntity> Entities { get; set; } = new();
        public DateTime LastModified { get; set; }
        public int TotalCount { get; set; }
        public string ChangeSetId { get; set; } = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Entity with sync metadata for cloud synchronization
    /// </summary>
    public class SyncEntity
    {
        public Guid LocalId { get; set; }
        public string? CloudId { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public string EntityData { get; set; } = string.Empty; // JSON serialized entity
        public SyncOperation Operation { get; set; }
        public DateTime LocalTimestamp { get; set; }
        public DateTime? CloudTimestamp { get; set; }
        public string Version { get; set; } = "1";
        public string Hash { get; set; } = string.Empty;
        public EntitySyncStatus Status { get; set; } = EntitySyncStatus.Pending;
        public List<string> ConflictFields { get; set; } = new();
    }

    /// <summary>
    /// Conflict information for resolution
    /// </summary>
    public class ConflictInfo
    {
        public Guid ConflictId { get; set; } = Guid.NewGuid();
        public string EntityType { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public string LocalData { get; set; } = string.Empty;
        public string CloudData { get; set; } = string.Empty;
        public string BaseData { get; set; } = string.Empty; // For three-way merge
        public List<FieldConflict> FieldConflicts { get; set; } = new();
        public DateTime ConflictTime { get; set; } = DateTime.UtcNow;
        public ConflictResolutionStrategy Strategy { get; set; } = ConflictResolutionStrategy.UserChoice;
        public string? Resolution { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolvedBy { get; set; }
    }

    /// <summary>
    /// Field-level conflict information
    /// </summary>
    public class FieldConflict
    {
        public string FieldName { get; set; } = string.Empty;
        public object? LocalValue { get; set; }
        public object? CloudValue { get; set; }
        public object? BaseValue { get; set; }
        public FieldConflictType ConflictType { get; set; }
        public string? Resolution { get; set; }
    }

    /// <summary>
    /// Sync metadata and tracking information
    /// </summary>
    public class SyncMetadata
    {
        public DateTime LastFullSync { get; set; }
        public DateTime LastDeltaSync { get; set; }
        public Dictionary<string, DateTime> EntityLastSync { get; set; } = new();
        public int TotalEntities { get; set; }
        public int SuccessfulEntities { get; set; }
        public int FailedEntities { get; set; }
        public int ConflictEntities { get; set; }
        public List<string> SyncedEntityTypes { get; set; } = new();
        public string ClientVersion { get; set; } = string.Empty;
        public string ServerVersion { get; set; } = string.Empty;
    }

    /// <summary>
    /// Sync status tracking entity
    /// </summary>
    public class SyncStatusTracker : BaseEntity
    {
        public string EntityType { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public EntitySyncStatus Status { get; set; } = EntitySyncStatus.Pending;
        public DateTime LastAttempt { get; set; }
        public DateTime? LastSuccess { get; set; }
        public int AttemptCount { get; set; } = 0;
        public string? ErrorMessage { get; set; }
        public string? CloudId { get; set; }
        public string EntityVersion { get; set; } = "1";
    }

    /// <summary>
    /// Change detection and tracking
    /// </summary>
    public class ChangeTracker : BaseEntity
    {
        public string EntityType { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public DateTime ChangeTime { get; set; } = DateTime.UtcNow;
        public string ChangeType { get; set; } = string.Empty; // Insert, Update, Delete
        public string UserId { get; set; } = string.Empty;
        public bool IsSynced { get; set; } = false;
    }

    /// <summary>
    /// Tombstone record for deleted entities
    /// </summary>
    public class TombstoneRecord : BaseEntity
    {
        public string EntityType { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public DateTime DeletedAt { get; set; } = DateTime.UtcNow;
        public string DeletedBy { get; set; } = string.Empty;
        public string? CloudId { get; set; }
        public bool IsSynced { get; set; } = false;
        public string EntityData { get; set; } = string.Empty; // Last known state before deletion
    }

    // Enums for sync operations
    public enum SyncOperation
    {
        Create,
        Update,
        Delete,
        Restore
    }

    public enum EntitySyncStatus
    {
        Pending,
        InProgress,
        Success,
        Failed,
        Conflict,
        Skipped
    }

    public enum ConflictResolutionStrategy
    {
        UserChoice,
        LocalWins,
        CloudWins,
        MergeFields,
        KeepBoth,
        Automatic
    }

    public enum FieldConflictType
    {
        ValueChanged,
        LocalDeleted,
        CloudDeleted,
        TypeMismatch,
        ValidationError
    }

    // Sync configuration and settings
    public class SyncConfiguration
    {
        public bool EnableAutoSync { get; set; } = true;
        public int SyncIntervalMinutes { get; set; } = 30;
        public int MaxRetryAttempts { get; set; } = 3;
        public int RetryDelayMinutes { get; set; } = 5;
        public bool EnableConflictResolution { get; set; } = true;
        public ConflictResolutionStrategy DefaultConflictStrategy { get; set; } = ConflictResolutionStrategy.UserChoice;
        public bool EnableDeltaSync { get; set; } = true;
        public int BatchSize { get; set; } = 100;
        public bool CompressPayloads { get; set; } = true;
        public List<string> ExcludedEntityTypes { get; set; } = new();
        public Dictionary<string, int> EntityPriorities { get; set; } = new();
    }
}
