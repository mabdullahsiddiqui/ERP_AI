using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERP_AI.Data;
using Microsoft.Extensions.Logging;
using RestSharp;
using Newtonsoft.Json;

namespace ERP_AI.CloudSync
{
    public class SyncService : ISyncService
    {
        private readonly ERPDbContext _context;
        private readonly ILogger<SyncService> _logger;
        private readonly CloudApiClient _apiClient;
        private readonly IConflictResolver _conflictResolver;

        public bool IsOnline => CheckConnectivity();
        public string LastSyncTime => GetLastSyncTime();
        public int PendingChangesCount => GetPendingChangesCount();

        public SyncService(ERPDbContext context, ILogger<SyncService> logger, IConflictResolver conflictResolver, CloudApiClient apiClient)
        {
            _context = context;
            _logger = logger;
            _conflictResolver = conflictResolver;
            _apiClient = apiClient;
        }

        public async Task<bool> SyncAsync()
        {
            try
            {
                _logger.LogInformation("Starting full synchronization");

                if (!IsOnline)
                {
                    _logger.LogWarning("Cannot sync: No internet connection");
                    return false;
                }

                // Step 1: Push local changes
                var pushResult = await PushChangesAsync();
                if (!pushResult)
                {
                    _logger.LogError("Failed to push local changes");
                    return false;
                }

                // Step 2: Pull remote changes
                var pullResult = await PullChangesAsync();
                if (!pullResult)
                {
                    _logger.LogError("Failed to pull remote changes");
                    return false;
                }

                // Step 3: Check for conflicts
                var conflicts = await DetectConflictsAsync();
                if (conflicts.Any())
                {
                    var resolutionResult = await ResolveConflictsAsync(conflicts);
                    if (!resolutionResult)
                    {
                        _logger.LogError("Failed to resolve conflicts");
                        return false;
                    }
                }

                // Step 4: Update sync timestamp
                await UpdateLastSyncTimeAsync();

                _logger.LogInformation("Synchronization completed successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during synchronization");
                return false;
            }
        }

        public async Task<bool> PushChangesAsync()
        {
            try
            {
                _logger.LogInformation("Pushing local changes to cloud");

                var pendingItems = _context.SyncQueues.Where(q => !q.IsDeleted).ToList();
                if (!pendingItems.Any())
                {
                    _logger.LogInformation("No pending changes to push");
                    return true;
                }

                foreach (var item in pendingItems)
                {
                    var success = await PushItemAsync(item);
                    if (success)
                    {
                        item.IsDeleted = true; // Mark as processed
                        _context.SyncQueues.Update(item);
                    }
                    else
                    {
                        _logger.LogError($"Failed to push item {item.EntityType}:{item.EntityId}");
                        return false;
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Successfully pushed {pendingItems.Count} changes");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pushing changes");
                return false;
            }
        }

        public async Task<bool> PullChangesAsync()
        {
            try
            {
                _logger.LogInformation("Pulling changes from cloud");

                var response = await _apiClient.GetSyncChangesAsync(LastSyncTime);
                if (!response.IsSuccess)
                {
                    _logger.LogError($"Failed to retrieve changes from cloud: {response.ErrorMessage}");
                    return false;
                }

                var changes = response.Data?.Changes ?? new List<SyncChange>();
                foreach (var change in changes)
                {
                    await ApplyRemoteChangeAsync(change);
                }

                _logger.LogInformation($"Successfully pulled {changes.Count} changes");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pulling changes");
                return false;
            }
        }

        public async Task<bool> ResolveConflictsAsync(IEnumerable<ConflictResolution> conflicts)
        {
            try
            {
                foreach (var conflict in conflicts)
                {
                    var resolved = await _conflictResolver.ResolveAsync(conflict);
                    if (!resolved)
                    {
                        _logger.LogError($"Failed to resolve conflict for {conflict.EntityType}:{conflict.EntityId}");
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving conflicts");
                return false;
            }
        }

        public async Task<IEnumerable<SyncLog>> GetSyncHistoryAsync(DateTime fromDate, DateTime toDate)
        {
            return await Task.FromResult(_context.SyncLogs
                .Where(log => log.CreatedAt >= fromDate && log.CreatedAt <= toDate)
                .OrderByDescending(log => log.CreatedAt)
                .ToList());
        }

        private async Task<bool> PushItemAsync(SyncQueue item)
        {
            try
            {
                var entity = await GetEntityByIdAsync(item.EntityType, item.EntityId);

                if (entity == null)
                {
                    _logger.LogWarning($"Entity {item.EntityType}:{item.EntityId} not found");
                    return false;
                }

                var response = await _apiClient.PushEntityAsync(item.EntityType, entity);

                if (response.IsSuccess && response.Data?.Success == true)
                {
                    // Update cloud mapping
                    var mapping = _context.CloudMappings
                        .FirstOrDefault(m => m.LocalId == item.EntityId && m.EntityType == item.EntityType);

                    if (mapping == null)
                    {
                        mapping = new CloudMapping
                        {
                            LocalId = item.EntityId,
                            EntityType = item.EntityType,
                            CloudId = response.Data.CloudId ?? Guid.NewGuid().ToString()
                        };
                        _context.CloudMappings.Add(mapping);
                    }
                    else
                    {
                        mapping.UpdatedAt = DateTime.Now;
                        _context.CloudMappings.Update(mapping);
                    }

                    await _context.SaveChangesAsync();

                    // Log successful sync
                    var log = new SyncLog
                    {
                        Operation = "Push",
                        EntityType = item.EntityType,
                        EntityId = item.EntityId,
                        Details = $"Successfully pushed to cloud"
                    };
                    _context.SyncLogs.Add(log);
                    await _context.SaveChangesAsync();

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error pushing item {item.EntityType}:{item.EntityId}");
                return false;
            }
        }

        private async Task ApplyRemoteChangeAsync(SyncChange change)
        {
            try
            {
                // Apply the remote change to local database
                // This is a simplified implementation
                _logger.LogInformation($"Applying remote change: {change.EntityType}:{change.EntityId}");

                var log = new SyncLog
                {
                    Operation = "Pull",
                    EntityType = change.EntityType,
                    EntityId = change.EntityId,
                    Details = $"Applied remote change"
                };
                _context.SyncLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error applying remote change {change.EntityType}:{change.EntityId}");
            }
        }

        private async Task<IEnumerable<ConflictResolution>> DetectConflictsAsync()
        {
            // Simplified conflict detection
            // In a real implementation, this would compare timestamps and content
            return await Task.FromResult(Enumerable.Empty<ConflictResolution>());
        }

        private bool CheckConnectivity()
        {
            // Simplified connectivity check
            return true; // Assume always online for demo
        }

        private string GetLastSyncTime()
        {
            var settings = _context.SyncSettings.FirstOrDefault();
            return settings?.LastSyncTime.ToString("O") ?? DateTime.MinValue.ToString("O");
        }

        private int GetPendingChangesCount()
        {
            return _context.SyncQueues.Count(q => !q.IsDeleted);
        }

        private async Task UpdateLastSyncTimeAsync()
        {
            var settings = _context.SyncSettings.FirstOrDefault();
            if (settings == null)
            {
                settings = new SyncSettings { LastSyncTime = DateTime.Now };
                _context.SyncSettings.Add(settings);
            }
            else
            {
                settings.LastSyncTime = DateTime.Now;
                _context.SyncSettings.Update(settings);
            }
            await _context.SaveChangesAsync();
        }

        private async Task<object?> GetEntityByIdAsync(string entityType, Guid entityId)
        {
            return entityType switch
            {
                "Account" => await _context.Accounts.FindAsync(entityId),
                "Customer" => await _context.Customers.FindAsync(entityId),
                "Vendor" => await _context.Vendors.FindAsync(entityId),
                "Invoice" => await _context.Invoices.FindAsync(entityId),
                "Bill" => await _context.Bills.FindAsync(entityId),
                "Transaction" => await _context.Transactions.FindAsync(entityId),
                _ => null
            };
        }
    }

    public class SyncChange
    {
        public string EntityType { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public string Action { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public object Data { get; set; } = null!;
    }
}

