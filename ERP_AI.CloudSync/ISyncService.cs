using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERP_AI.Data;

namespace ERP_AI.CloudSync
{
    public interface ISyncService
    {
        Task<bool> SyncAsync();
        Task<bool> PushChangesAsync();
        Task<bool> PullChangesAsync();
        Task<bool> ResolveConflictsAsync(IEnumerable<ConflictResolution> conflicts);
        Task<IEnumerable<SyncLog>> GetSyncHistoryAsync(DateTime fromDate, DateTime toDate);
        bool IsOnline { get; }
        string LastSyncTime { get; }
        int PendingChangesCount { get; }
    }
}

