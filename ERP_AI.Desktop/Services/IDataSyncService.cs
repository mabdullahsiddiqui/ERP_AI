using ERP_AI.Core;
using ERP_AI.Data;

namespace ERP_AI.Desktop.Services
{
    public interface IDataSyncService
    {
        // Sync Status
        event EventHandler<SyncProgressEventArgs>? SyncProgress;
        event EventHandler<SyncCompletedEventArgs>? SyncCompleted;
        event EventHandler<SyncErrorEventArgs>? SyncError;

        // Properties
        bool IsOnline { get; }
        bool IsSyncing { get; }
        DateTime LastSyncTime { get; }
        SyncStatus CurrentStatus { get; }

        // Core Sync Methods
        Task<bool> SyncAllDataAsync();
        Task<bool> SyncAccountsAsync();
        Task<bool> SyncTransactionsAsync();
        Task<bool> SyncCustomersAsync();
        Task<bool> SyncVendorsAsync();
        Task<bool> SyncInvoicesAsync();
        Task<bool> SyncBillsAsync();

        // Conflict Resolution
        Task<List<SyncConflict>> GetConflictsAsync();
        Task<bool> ResolveConflictAsync(string conflictId, ConflictResolution resolution);
        Task<bool> ResolveAllConflictsAsync(ConflictResolution defaultResolution);

        // Offline Support
        Task<bool> QueueOfflineActionAsync(OfflineAction action);
        Task<List<OfflineAction>> GetPendingActionsAsync();
        Task<bool> ProcessPendingActionsAsync();

        // Data Management
        Task<bool> UploadLocalChangesAsync();
        Task<bool> DownloadRemoteChangesAsync();
        Task<bool> ClearLocalDataAsync();
        Task<bool> ResetSyncStatusAsync();

        // Health Check
        Task<SyncHealthStatus> GetSyncHealthAsync();
        Task<bool> ValidateDataIntegrityAsync();
    }

    public class SyncProgressEventArgs : EventArgs
    {
        public string EntityType { get; set; } = string.Empty;
        public int Processed { get; set; }
        public int Total { get; set; }
        public double Percentage => Total > 0 ? (double)Processed / Total * 100 : 0;
        public string Message { get; set; } = string.Empty;
    }

    public class SyncCompletedEventArgs : EventArgs
    {
        public bool Success { get; set; }
        public int TotalProcessed { get; set; }
        public int ConflictsFound { get; set; }
        public TimeSpan Duration { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class SyncErrorEventArgs : EventArgs
    {
        public string EntityType { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public Exception? Exception { get; set; }
        public bool CanRetry { get; set; } = true;
    }

    public class SyncConflict
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public object LocalData { get; set; } = new();
        public object RemoteData { get; set; } = new();
        public DateTime ConflictTime { get; set; } = DateTime.UtcNow;
        public ConflictType Type { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class OfflineAction
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ActionType { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public object Data { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int RetryCount { get; set; } = 0;
        public string? ErrorMessage { get; set; }
    }

    public enum ConflictType
    {
        UpdateConflict,
        DeleteConflict,
        CreateConflict,
        DataMismatch
    }

    public enum ConflictResolution
    {
        UseLocal,
        UseRemote,
        Merge,
        Skip
    }

    public enum SyncStatus
    {
        Idle,
        Syncing,
        Paused,
        Error,
        Offline
    }

    public class SyncHealthStatus
    {
        public bool IsHealthy { get; set; }
        public int PendingConflicts { get; set; }
        public int FailedSyncs { get; set; }
        public int PendingOfflineActions { get; set; }
        public DateTime LastSuccessfulSync { get; set; }
        public List<string> Issues { get; set; } = new();
    }
}
