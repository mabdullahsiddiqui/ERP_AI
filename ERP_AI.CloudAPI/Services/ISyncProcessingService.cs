using ERP_AI.CloudAPI.Models;
using ERP_AI.Data;

namespace ERP_AI.CloudAPI.Services
{
    public interface ISyncProcessingService
    {
        /// <summary>
        /// Process sync upload from desktop client
        /// </summary>
        Task<SyncUploadResponse> ProcessUploadAsync(SyncUploadRequest request, string userId);

        /// <summary>
        /// Process sync download to desktop client
        /// </summary>
        Task<SyncDownloadResponse> ProcessDownloadAsync(SyncDownloadRequest request, string userId);

        /// <summary>
        /// Resolve sync conflict
        /// </summary>
        Task<ConflictResolutionResponse> ResolveConflictAsync(ConflictResolutionRequest request, string userId);

        /// <summary>
        /// Get sync status for company
        /// </summary>
        Task<SyncStatusResponse> GetSyncStatusAsync(SyncStatusRequest request, string userId);

        /// <summary>
        /// Get pending conflicts for company
        /// </summary>
        Task<List<ConflictInfo>> GetPendingConflictsAsync(string companyId, string? entityType = null);

        /// <summary>
        /// Process batch sync operations
        /// </summary>
        Task<BatchSyncResponse> ProcessBatchSyncAsync(BatchSyncRequest request, string userId);

        /// <summary>
        /// Get sync history
        /// </summary>
        Task<SyncHistoryResponse> GetSyncHistoryAsync(SyncHistoryRequest request);

        /// <summary>
        /// Get health status of sync services
        /// </summary>
        Task<Dictionary<string, object>> GetHealthStatusAsync();

        /// <summary>
        /// Detect conflicts between local and cloud data
        /// </summary>
        Task<List<ConflictInfo>> DetectConflictsAsync(List<SyncEntity> entities, string companyId);

        /// <summary>
        /// Apply three-way merge for conflict resolution
        /// </summary>
        Task<SyncEntity?> ApplyThreeWayMergeAsync(ConflictInfo conflict, ConflictResolutionStrategy strategy);

        /// <summary>
        /// Validate sync package integrity
        /// </summary>
        Task<bool> ValidateSyncPackageAsync(SyncPackage package);

        /// <summary>
        /// Transform entities for cloud storage
        /// </summary>
        Task<List<SyncEntity>> TransformEntitiesAsync(List<SyncEntity> entities, string companyId);
    }
}
