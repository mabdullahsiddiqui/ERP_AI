using System.ComponentModel.DataAnnotations;
using ERP_AI.Data;

namespace ERP_AI.CloudAPI.Models
{
    // Cloud API Sync Models for Phase 3
    
    public class SyncUploadRequest
    {
        [Required]
        public string CompanyId { get; set; } = string.Empty;
        
        [Required]
        public SyncPackage Package { get; set; } = new();
        
        public bool ForceOverwrite { get; set; } = false;
        public string ClientVersion { get; set; } = string.Empty;
    }

    public class SyncUploadResponse
    {
        public bool Success { get; set; }
        public string SyncId { get; set; } = string.Empty;
        public int ProcessedEntities { get; set; }
        public int SuccessfulEntities { get; set; }
        public int FailedEntities { get; set; }
        public int ConflictEntities { get; set; }
        public List<SyncError> Errors { get; set; } = new();
        public List<ConflictInfo> Conflicts { get; set; } = new();
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
        public string? ErrorMessage { get; set; }
    }

    public class SyncDownloadRequest
    {
        [Required]
        public string CompanyId { get; set; } = string.Empty;
        
        public DateTime? LastSyncTime { get; set; }
        public List<string>? EntityTypes { get; set; }
        public int MaxEntities { get; set; } = 1000;
        public bool IncludeDeleted { get; set; } = true;
        public string ClientVersion { get; set; } = string.Empty;
    }

    public class SyncDownloadResponse
    {
        public bool Success { get; set; }
        public SyncPackage Package { get; set; } = new();
        public bool HasMoreData { get; set; }
        public string? NextPageToken { get; set; }
        public DateTime ServerTime { get; set; } = DateTime.UtcNow;
        public string? ErrorMessage { get; set; }
    }

    public class ConflictResolutionRequest
    {
        [Required]
        public string ConflictId { get; set; } = string.Empty;
        
        [Required]
        public string CompanyId { get; set; } = string.Empty;
        
        [Required]
        public ConflictResolutionStrategy Strategy { get; set; }
        
        public string? ResolvedData { get; set; }
        public Dictionary<string, object>? FieldResolutions { get; set; }
    }

    public class ConflictResolutionResponse
    {
        public bool Success { get; set; }
        public string ConflictId { get; set; } = string.Empty;
        public SyncEntity? ResolvedEntity { get; set; }
        public DateTime ResolvedAt { get; set; } = DateTime.UtcNow;
        public string? ErrorMessage { get; set; }
    }

    public class SyncStatusRequest
    {
        [Required]
        public string CompanyId { get; set; } = string.Empty;
        
        public string? SyncId { get; set; }
        public List<string>? EntityTypes { get; set; }
    }

    public class SyncStatusResponse
    {
        public bool Success { get; set; }
        public string CompanyId { get; set; } = string.Empty;
        public DateTime LastSyncTime { get; set; }
        public SyncHealthStatus OverallStatus { get; set; } = SyncHealthStatus.Healthy;
        public List<EntitySyncStatus> EntityStatuses { get; set; } = new();
        public int PendingConflicts { get; set; }
        public int FailedSyncs { get; set; }
        public DateTime ServerTime { get; set; } = DateTime.UtcNow;
        public string? ErrorMessage { get; set; }
    }

    public class EntitySyncStatus
    {
        public string EntityType { get; set; } = string.Empty;
        public DateTime LastSyncTime { get; set; }
        public int TotalEntities { get; set; }
        public int SyncedEntities { get; set; }
        public int PendingEntities { get; set; }
        public int FailedEntities { get; set; }
        public int ConflictEntities { get; set; }
        public SyncHealthStatus Status { get; set; } = SyncHealthStatus.Healthy;
    }

    public class SyncError
    {
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string? StackTrace { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
        public SyncErrorSeverity Severity { get; set; } = SyncErrorSeverity.Error;
    }

    public class BatchSyncRequest
    {
        [Required]
        public string CompanyId { get; set; } = string.Empty;
        
        [Required]
        public List<SyncPackage> Packages { get; set; } = new();
        
        public bool ContinueOnError { get; set; } = true;
        public int MaxConcurrency { get; set; } = 5;
    }

    public class BatchSyncResponse
    {
        public bool Success { get; set; }
        public int TotalPackages { get; set; }
        public int SuccessfulPackages { get; set; }
        public int FailedPackages { get; set; }
        public List<SyncUploadResponse> Results { get; set; } = new();
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
        public string? ErrorMessage { get; set; }
    }

    public class SyncHistoryRequest
    {
        [Required]
        public string CompanyId { get; set; } = string.Empty;
        
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<string>? EntityTypes { get; set; }
        public int PageSize { get; set; } = 50;
        public int PageNumber { get; set; } = 1;
    }

    public class SyncHistoryResponse
    {
        public bool Success { get; set; }
        public List<SyncHistoryEntry> Entries { get; set; } = new();
        public int TotalEntries { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public bool HasNextPage { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class SyncHistoryEntry
    {
        public string SyncId { get; set; } = string.Empty;
        public DateTime SyncTime { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public SyncOperation Operation { get; set; }
        public EntitySyncStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string ClientVersion { get; set; } = string.Empty;
    }

    // Enums for Cloud API
    public enum SyncHealthStatus
    {
        Healthy,
        Warning,
        Critical,
        Unknown
    }

    public enum SyncErrorSeverity
    {
        Info,
        Warning,
        Error,
        Critical
    }

    // API Response Wrapper
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorCode { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Version { get; set; } = "1.0";
    }

    public class ApiResponse : ApiResponse<object>
    {
        public static ApiResponse<T> SuccessResult<T>(T data)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data
            };
        }

        public static ApiResponse<T> ErrorResult<T>(string errorMessage, string? errorCode = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                ErrorMessage = errorMessage,
                ErrorCode = errorCode
            };
        }
    }
}
