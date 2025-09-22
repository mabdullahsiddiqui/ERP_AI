using ERP_AI.Core;
using ERP_AI.Data;

namespace ERP_AI.Desktop.Services
{
    public interface IIntegrationService
    {
        // Service Status
        bool IsInitialized { get; }
        bool IsOnline { get; }
        DateTime LastSyncTime { get; }
        IntegrationStatus Status { get; }

        // Events
        event EventHandler<IntegrationStatusChangedEventArgs>? StatusChanged;
        event EventHandler<SyncProgressEventArgs>? SyncProgress;
        event EventHandler<DataChangedEventArgs>? DataChanged;
        event EventHandler<ErrorEventArgs>? ErrorOccurred;

        // Initialization
        Task<bool> InitializeAsync();
        Task<bool> InitializeServicesAsync();
        Task<bool> ValidateConfigurationAsync();
        Task<bool> TestConnectionsAsync();

        // Authentication Integration
        Task<bool> AuthenticateAsync(string email, string password);
        Task<bool> RegisterAsync(string email, string password, string companyName);
        Task<bool> LogoutAsync();
        Task<bool> RefreshAuthenticationAsync();

        // Data Synchronization
        Task<bool> SyncAllDataAsync();
        Task<bool> SyncEntityAsync<T>(string entityType) where T : BaseEntity;
        Task<bool> UploadLocalChangesAsync();
        Task<bool> DownloadRemoteChangesAsync();
        Task<bool> ResolveConflictsAsync();

        // Real-time Updates
        Task<bool> StartRealTimeSyncAsync();
        Task<bool> StopRealTimeSyncAsync();
        Task<bool> SubscribeToUpdatesAsync(string entityType);
        Task<bool> UnsubscribeFromUpdatesAsync(string entityType);

        // Offline Support
        Task<bool> EnableOfflineModeAsync();
        Task<bool> DisableOfflineModeAsync();
        Task<bool> ProcessOfflineQueueAsync();
        Task<bool> IsOfflineDataAvailableAsync();

        // Data Management
        Task<bool> BackupDataAsync(string backupPath);
        Task<bool> RestoreDataAsync(string backupPath);
        Task<bool> ClearLocalDataAsync();
        Task<bool> ValidateDataIntegrityAsync();

        // Performance Monitoring
        Task<PerformanceMetrics> GetPerformanceMetricsAsync();
        Task<bool> OptimizePerformanceAsync();
        Task<bool> ClearCacheAsync();

        // Error Handling
        Task<bool> HandleErrorAsync(Exception exception, string context);
        Task<bool> RetryFailedOperationsAsync();
        Task<List<ErrorLog>> GetErrorLogsAsync();

        // Health Check
        Task<HealthStatus> GetHealthStatusAsync();
        Task<bool> IsServiceHealthyAsync(string serviceName);
        Task<Dictionary<string, bool>> GetAllServicesHealthAsync();

        // Configuration
        Task<bool> UpdateConfigurationAsync(Dictionary<string, object> settings);
        Task<Dictionary<string, object>> GetConfigurationAsync();
        Task<bool> ResetToDefaultsAsync();
    }

    public class IntegrationStatusChangedEventArgs : EventArgs
    {
        public IntegrationStatus OldStatus { get; set; }
        public IntegrationStatus NewStatus { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class DataChangedEventArgs : EventArgs
    {
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public DataChangeType ChangeType { get; set; }
        public object? Data { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class ErrorEventArgs : EventArgs
    {
        public string Service { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public Exception? Exception { get; set; }
        public bool CanRetry { get; set; } = true;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public enum IntegrationStatus
    {
        NotInitialized,
        Initializing,
        Online,
        Offline,
        Syncing,
        Error,
        Maintenance
    }

    public enum DataChangeType
    {
        Created,
        Updated,
        Deleted,
        Synced
    }

    public class PerformanceMetrics
    {
        public TimeSpan AverageResponseTime { get; set; }
        public int RequestsPerSecond { get; set; }
        public int ErrorRate { get; set; }
        public long MemoryUsage { get; set; }
        public int ActiveConnections { get; set; }
        public int CachedItems { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class HealthStatus
    {
        public bool IsHealthy { get; set; }
        public List<ServiceHealth> Services { get; set; } = new();
        public List<string> Issues { get; set; } = new();
        public DateTime LastCheck { get; set; } = DateTime.UtcNow;
        public string OverallStatus { get; set; } = string.Empty;
    }

    public class ServiceHealth
    {
        public string Name { get; set; } = string.Empty;
        public bool IsHealthy { get; set; }
        public string Status { get; set; } = string.Empty;
        public TimeSpan ResponseTime { get; set; }
        public DateTime LastCheck { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class ErrorLog
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Service { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string StackTrace { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsResolved { get; set; }
        public string? Resolution { get; set; }
    }
}
