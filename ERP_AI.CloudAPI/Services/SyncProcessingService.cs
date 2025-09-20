using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ERP_AI.CloudAPI.Models;
using ERP_AI.Data;
using System.Security.Cryptography;
using System.Text;

namespace ERP_AI.CloudAPI.Services
{
    public class SyncProcessingService : ISyncProcessingService
    {
        private readonly ERPDbContext _context;
        private readonly ILogger<SyncProcessingService> _logger;

        public SyncProcessingService(ERPDbContext context, ILogger<SyncProcessingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<SyncUploadResponse> ProcessUploadAsync(SyncUploadRequest request, string userId)
        {
            var response = new SyncUploadResponse
            {
                SyncId = Guid.NewGuid().ToString(),
                ProcessedAt = DateTime.UtcNow
            };

            try
            {
                _logger.LogInformation("Processing sync upload for company {CompanyId} with {ChangeSetCount} change sets",
                    request.CompanyId, request.Package.ChangeSets.Count);

                // Validate sync package
                if (!await ValidateSyncPackageAsync(request.Package))
                {
                    response.Success = false;
                    response.ErrorMessage = "Invalid sync package";
                    return response;
                }

                var allEntities = request.Package.ChangeSets.SelectMany(cs => cs.Entities).ToList();
                response.ProcessedEntities = allEntities.Count;

                // Transform entities for cloud storage
                var transformedEntities = await TransformEntitiesAsync(allEntities, request.CompanyId);

                // Detect conflicts
                var conflicts = await DetectConflictsAsync(transformedEntities, request.CompanyId);
                response.ConflictEntities = conflicts.Count;
                response.Conflicts = conflicts;

                // Process each entity
                foreach (var entity in transformedEntities)
                {
                    try
                    {
                        if (conflicts.Any(c => c.EntityId == entity.LocalId))
                        {
                            // Skip conflicted entities for now
                            continue;
                        }

                        await ProcessEntityAsync(entity, request.CompanyId, userId);
                        response.SuccessfulEntities++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing entity {EntityId} of type {EntityType}",
                            entity.LocalId, entity.EntityType);
                        
                        response.Errors.Add(new SyncError
                        {
                            EntityType = entity.EntityType,
                            EntityId = entity.LocalId.ToString(),
                            ErrorCode = "PROCESSING_ERROR",
                            ErrorMessage = ex.Message,
                            Severity = SyncErrorSeverity.Error
                        });
                        response.FailedEntities++;
                    }
                }

                // Log sync operation
                await LogSyncOperationAsync(response.SyncId, request.CompanyId, userId, "Upload", 
                    response.SuccessfulEntities, response.FailedEntities, response.ConflictEntities);

                response.Success = response.FailedEntities == 0;
                
                _logger.LogInformation("Sync upload completed: {SuccessfulEntities} successful, {FailedEntities} failed, {ConflictEntities} conflicts",
                    response.SuccessfulEntities, response.FailedEntities, response.ConflictEntities);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing sync upload for company {CompanyId}", request.CompanyId);
                response.Success = false;
                response.ErrorMessage = ex.Message;
                return response;
            }
        }

        public async Task<SyncDownloadResponse> ProcessDownloadAsync(SyncDownloadRequest request, string userId)
        {
            var response = new SyncDownloadResponse
            {
                Package = new SyncPackage
                {
                    CompanyId = request.CompanyId,
                    UserId = userId,
                    Timestamp = DateTime.UtcNow
                },
                ServerTime = DateTime.UtcNow
            };

            try
            {
                _logger.LogInformation("Processing sync download for company {CompanyId} since {LastSyncTime}",
                    request.CompanyId, request.LastSyncTime);

                var lastSyncTime = request.LastSyncTime ?? DateTime.MinValue;
                var entityTypes = request.EntityTypes ?? GetAllEntityTypes();

                // Get changes for each entity type
                foreach (var entityType in entityTypes)
                {
                    var changeSet = await GetChangeSetAsync(entityType, request.CompanyId, lastSyncTime, request.MaxEntities);
                    if (changeSet.Entities.Any())
                    {
                        response.Package.ChangeSets.Add(changeSet);
                    }
                }

                // Check if there's more data
                var totalChanges = response.Package.ChangeSets.Sum(cs => cs.Entities.Count);
                response.HasMoreData = totalChanges >= request.MaxEntities;

                // Set metadata
                response.Package.Metadata = new SyncMetadata
                {
                    LastFullSync = lastSyncTime,
                    LastDeltaSync = DateTime.UtcNow,
                    TotalEntities = totalChanges,
                    SuccessfulEntities = totalChanges,
                    SyncedEntityTypes = entityTypes.ToList(),
                    ClientVersion = request.ClientVersion,
                    ServerVersion = "1.0"
                };

                response.Success = true;

                _logger.LogInformation("Sync download completed: {TotalEntities} entities returned", totalChanges);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing sync download for company {CompanyId}", request.CompanyId);
                response.Success = false;
                response.ErrorMessage = ex.Message;
                return response;
            }
        }

        public async Task<ConflictResolutionResponse> ResolveConflictAsync(ConflictResolutionRequest request, string userId)
        {
            var response = new ConflictResolutionResponse
            {
                ConflictId = request.ConflictId
            };

            try
            {
                var conflict = await _context.ConflictResolutions
                    .FirstOrDefaultAsync(c => c.Id.ToString() == request.ConflictId && c.Status == "Pending");

                if (conflict == null)
                {
                    response.Success = false;
                    response.ErrorMessage = "Conflict not found or already resolved";
                    return response;
                }

                // Apply resolution strategy
                var conflictInfo = new ConflictInfo
                {
                    ConflictId = Guid.Parse(request.ConflictId),
                    EntityType = conflict.EntityType,
                    EntityId = conflict.EntityId,
                    LocalData = conflict.LocalData,
                    CloudData = conflict.CloudData
                };

                var resolvedEntity = await ApplyThreeWayMergeAsync(conflictInfo, request.Strategy);

                if (resolvedEntity != null)
                {
                    // Update conflict record
                    conflict.Status = "Resolved";
                    conflict.ResolvedAt = DateTime.UtcNow;
                    conflict.ResolvedBy = userId;
                    conflict.Resolution = JsonConvert.SerializeObject(resolvedEntity);

                    await _context.SaveChangesAsync();

                    response.Success = true;
                    response.ResolvedEntity = resolvedEntity;
                    response.ResolvedAt = conflict.ResolvedAt;
                }
                else
                {
                    response.Success = false;
                    response.ErrorMessage = "Failed to resolve conflict";
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving conflict {ConflictId}", request.ConflictId);
                response.Success = false;
                response.ErrorMessage = ex.Message;
                return response;
            }
        }

        public async Task<SyncStatusResponse> GetSyncStatusAsync(SyncStatusRequest request, string userId)
        {
            var response = new SyncStatusResponse
            {
                CompanyId = request.CompanyId,
                ServerTime = DateTime.UtcNow
            };

            try
            {
                // Get last sync time
                var settings = await _context.SyncSettings
                    .FirstOrDefaultAsync(s => s.CompanyId == request.CompanyId);

                response.LastSyncTime = settings?.LastSyncTime ?? DateTime.MinValue;

                // Get pending conflicts count
                response.PendingConflicts = await _context.ConflictResolutions
                    .CountAsync(c => c.Status == "Pending");

                // Get failed syncs count
                response.FailedSyncs = await _context.SyncQueues
                    .CountAsync(q => q.Status == "Failed");

                // Get entity statuses
                var entityTypes = request.EntityTypes ?? GetAllEntityTypes();
                foreach (var entityType in entityTypes)
                {
                    var entityStatus = await GetEntitySyncStatusAsync(entityType, request.CompanyId);
                    response.EntityStatuses.Add(entityStatus);
                }

                // Determine overall status
                response.OverallStatus = DetermineOverallStatus(response.EntityStatuses, response.PendingConflicts, response.FailedSyncs);

                response.Success = true;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sync status for company {CompanyId}", request.CompanyId);
                response.Success = false;
                response.ErrorMessage = ex.Message;
                return response;
            }
        }

        public async Task<List<ConflictInfo>> GetPendingConflictsAsync(string companyId, string? entityType = null)
        {
            var query = _context.ConflictResolutions
                .Where(c => c.Status == "Pending");

            if (!string.IsNullOrEmpty(entityType))
            {
                query = query.Where(c => c.EntityType == entityType);
            }

            var conflicts = await query.ToListAsync();

            return conflicts.Select(c => new ConflictInfo
            {
                ConflictId = c.Id,
                EntityType = c.EntityType,
                EntityId = c.EntityId,
                LocalData = c.LocalData,
                CloudData = c.CloudData,
                ConflictTime = c.CreatedAt
            }).ToList();
        }

        public async Task<BatchSyncResponse> ProcessBatchSyncAsync(BatchSyncRequest request, string userId)
        {
            var response = new BatchSyncResponse
            {
                TotalPackages = request.Packages.Count,
                ProcessedAt = DateTime.UtcNow
            };

            var semaphore = new SemaphoreSlim(request.MaxConcurrency, request.MaxConcurrency);
            var tasks = new List<Task<SyncUploadResponse>>();

            foreach (var package in request.Packages)
            {
                tasks.Add(ProcessPackageWithSemaphore(package, request.CompanyId, userId, semaphore, request.ContinueOnError));
            }

            response.Results = (await Task.WhenAll(tasks)).ToList();
            response.SuccessfulPackages = response.Results.Count(r => r.Success);
            response.FailedPackages = response.Results.Count(r => !r.Success);
            response.Success = response.FailedPackages == 0 || request.ContinueOnError;

            return response;
        }

        public async Task<SyncHistoryResponse> GetSyncHistoryAsync(SyncHistoryRequest request)
        {
            var query = _context.SyncLogs.AsQueryable();

            if (request.FromDate.HasValue)
                query = query.Where(l => l.SyncTime >= request.FromDate);

            if (request.ToDate.HasValue)
                query = query.Where(l => l.SyncTime <= request.ToDate);

            if (request.EntityTypes?.Any() == true)
                query = query.Where(l => request.EntityTypes.Contains(l.EntityType));

            var totalEntries = await query.CountAsync();

            var entries = await query
                .OrderByDescending(l => l.SyncTime)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(l => new SyncHistoryEntry
                {
                    SyncId = l.Id.ToString(),
                    SyncTime = l.SyncTime,
                    EntityType = l.EntityType,
                    EntityId = l.EntityId.ToString(),
                    Operation = Enum.Parse<SyncOperation>(l.Operation),
                    Status = Enum.Parse<EntitySyncStatus>(l.Status)
                })
                .ToListAsync();

            return new SyncHistoryResponse
            {
                Success = true,
                Entries = entries,
                TotalEntries = totalEntries,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                HasNextPage = (request.PageNumber * request.PageSize) < totalEntries
            };
        }

        public async Task<Dictionary<string, object>> GetHealthStatusAsync()
        {
            var health = new Dictionary<string, object>();

            try
            {
                // Database connectivity
                health["database"] = await _context.Database.CanConnectAsync() ? "Healthy" : "Unhealthy";

                // Pending sync queue count
                var pendingCount = await _context.SyncQueues.CountAsync(q => q.Status == "Pending");
                health["pending_sync_count"] = pendingCount;
                health["sync_queue_status"] = pendingCount > 1000 ? "Warning" : "Healthy";

                // Conflict count
                var conflictCount = await _context.ConflictResolutions.CountAsync(c => c.Status == "Pending");
                health["pending_conflicts"] = conflictCount;
                health["conflict_status"] = conflictCount > 100 ? "Warning" : "Healthy";

                // Failed syncs in last hour
                var failedSyncs = await _context.SyncLogs
                    .CountAsync(l => l.Status == "Failed" && l.SyncTime >= DateTime.UtcNow.AddHours(-1));
                health["failed_syncs_last_hour"] = failedSyncs;
                health["sync_reliability"] = failedSyncs > 10 ? "Warning" : "Healthy";

                health["overall_status"] = "Healthy";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                health["overall_status"] = "Unhealthy";
                health["error"] = ex.Message;
            }

            return health;
        }

        public async Task<List<ConflictInfo>> DetectConflictsAsync(List<SyncEntity> entities, string companyId)
        {
            var conflicts = new List<ConflictInfo>();

            foreach (var entity in entities)
            {
                // Check if entity has been modified on server since client's last sync
                var cloudMapping = await _context.CloudMappings
                    .FirstOrDefaultAsync(m => m.LocalId == entity.LocalId && m.EntityType == entity.EntityType);

                if (cloudMapping != null && entity.CloudTimestamp.HasValue)
                {
                    if (cloudMapping.LastSynced > entity.CloudTimestamp)
                    {
                        // Conflict detected
                        var conflict = new ConflictInfo
                        {
                            EntityType = entity.EntityType,
                            EntityId = entity.LocalId,
                            LocalData = entity.EntityData,
                            CloudData = await GetCloudEntityDataAsync(entity.EntityType, cloudMapping.CloudId),
                            ConflictTime = DateTime.UtcNow,
                            Strategy = ConflictResolutionStrategy.UserChoice
                        };

                        conflicts.Add(conflict);

                        // Save conflict to database
                        var conflictRecord = new ConflictResolution
                        {
                            EntityType = entity.EntityType,
                            EntityId = entity.LocalId,
                            LocalData = conflict.LocalData,
                            CloudData = conflict.CloudData,
                            ConflictType = "ModificationConflict",
                            Status = "Pending"
                        };

                        _context.ConflictResolutions.Add(conflictRecord);
                    }
                }
            }

            if (conflicts.Any())
            {
                await _context.SaveChangesAsync();
            }

            return conflicts;
        }

        public async Task<SyncEntity?> ApplyThreeWayMergeAsync(ConflictInfo conflict, ConflictResolutionStrategy strategy)
        {
            try
            {
                switch (strategy)
                {
                    case ConflictResolutionStrategy.LocalWins:
                        return JsonConvert.DeserializeObject<SyncEntity>(conflict.LocalData);

                    case ConflictResolutionStrategy.CloudWins:
                        return JsonConvert.DeserializeObject<SyncEntity>(conflict.CloudData);

                    case ConflictResolutionStrategy.MergeFields:
                        return await PerformFieldMergeAsync(conflict);

                    case ConflictResolutionStrategy.UserChoice:
                        // Return null to indicate user intervention required
                        return null;

                    default:
                        return JsonConvert.DeserializeObject<SyncEntity>(conflict.CloudData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying three-way merge for conflict {ConflictId}", conflict.ConflictId);
                return null;
            }
        }

        public async Task<bool> ValidateSyncPackageAsync(SyncPackage package)
        {
            // Validate package structure
            if (package.ChangeSets == null || !package.ChangeSets.Any())
                return false;

            // Validate checksum if provided
            if (!string.IsNullOrEmpty(package.Checksum))
            {
                var calculatedChecksum = CalculatePackageChecksum(package);
                if (calculatedChecksum != package.Checksum)
                    return false;
            }

            // Validate each entity
            foreach (var changeSet in package.ChangeSets)
            {
                foreach (var entity in changeSet.Entities)
                {
                    if (!IsValidEntity(entity))
                        return false;
                }
            }

            return true;
        }

        public async Task<List<SyncEntity>> TransformEntitiesAsync(List<SyncEntity> entities, string companyId)
        {
            var transformedEntities = new List<SyncEntity>();

            foreach (var entity in entities)
            {
                var transformed = entity;
                
                // Add company context
                var entityData = JsonConvert.DeserializeObject<Dictionary<string, object>>(entity.EntityData) 
                    ?? new Dictionary<string, object>();
                
                if (!entityData.ContainsKey("CompanyId"))
                {
                    entityData["CompanyId"] = companyId;
                    transformed.EntityData = JsonConvert.SerializeObject(entityData);
                }

                // Update timestamps
                transformed.CloudTimestamp = DateTime.UtcNow;

                // Generate hash for integrity
                transformed.Hash = CalculateEntityHash(transformed);

                transformedEntities.Add(transformed);
            }

            return transformedEntities;
        }

        // Private helper methods
        private async Task ProcessEntityAsync(SyncEntity entity, string companyId, string userId)
        {
            // This would contain the actual business logic for processing different entity types
            // For now, we'll just update the cloud mapping
            var mapping = await _context.CloudMappings
                .FirstOrDefaultAsync(m => m.LocalId == entity.LocalId && m.EntityType == entity.EntityType);

            if (mapping == null)
            {
                mapping = new CloudMapping
                {
                    LocalId = entity.LocalId,
                    CloudId = entity.CloudId ?? Guid.NewGuid().ToString(),
                    EntityType = entity.EntityType,
                    LastSynced = DateTime.UtcNow,
                    SyncStatus = "Synced"
                };
                _context.CloudMappings.Add(mapping);
            }
            else
            {
                mapping.LastSynced = DateTime.UtcNow;
                mapping.SyncStatus = "Synced";
                _context.CloudMappings.Update(mapping);
            }

            await _context.SaveChangesAsync();
        }

        private async Task<ChangeSet> GetChangeSetAsync(string entityType, string companyId, DateTime lastSyncTime, int maxEntities)
        {
            // This would contain logic to get changes for specific entity types
            // For now, return empty changeset
            return new ChangeSet
            {
                EntityType = entityType,
                LastModified = DateTime.UtcNow,
                ChangeSetId = Guid.NewGuid().ToString(),
                Entities = new List<SyncEntity>()
            };
        }

        private List<string> GetAllEntityTypes()
        {
            return new List<string>
            {
                "Account", "Transaction", "Customer", "Vendor", "Invoice", "Bill", "Payment"
            };
        }

        private async Task LogSyncOperationAsync(string syncId, string companyId, string userId, string operation, 
            int successful, int failed, int conflicts)
        {
            var syncLog = new SyncLog
            {
                Operation = operation,
                EntityType = "SyncOperation",
                EntityId = Guid.Parse(syncId),
                Details = JsonConvert.SerializeObject(new { successful, failed, conflicts, companyId, userId }),
                Status = failed == 0 ? "Success" : "PartialSuccess",
                SyncTime = DateTime.UtcNow
            };

            _context.SyncLogs.Add(syncLog);
            await _context.SaveChangesAsync();
        }

        private async Task<Models.EntitySyncStatus> GetEntitySyncStatusAsync(string entityType, string companyId)
        {
            // This would contain logic to get sync status for specific entity types
            return new Models.EntitySyncStatus
            {
                EntityType = entityType,
                LastSyncTime = DateTime.UtcNow.AddHours(-1),
                TotalEntities = 0,
                SyncedEntities = 0,
                PendingEntities = 0,
                FailedEntities = 0,
                ConflictEntities = 0,
                Status = SyncHealthStatus.Healthy
            };
        }

        private SyncHealthStatus DetermineOverallStatus(List<Models.EntitySyncStatus> entityStatuses, int pendingConflicts, int failedSyncs)
        {
            if (failedSyncs > 10 || pendingConflicts > 100)
                return SyncHealthStatus.Critical;

            if (failedSyncs > 0 || pendingConflicts > 0)
                return SyncHealthStatus.Warning;

            return SyncHealthStatus.Healthy;
        }

        private async Task<SyncUploadResponse> ProcessPackageWithSemaphore(SyncPackage package, string companyId, 
            string userId, SemaphoreSlim semaphore, bool continueOnError)
        {
            await semaphore.WaitAsync();
            try
            {
                var request = new SyncUploadRequest
                {
                    CompanyId = companyId,
                    Package = package
                };
                return await ProcessUploadAsync(request, userId);
            }
            finally
            {
                semaphore.Release();
            }
        }

        private async Task<string> GetCloudEntityDataAsync(string entityType, string cloudId)
        {
            // This would contain logic to get entity data from cloud storage
            return "{}"; // Placeholder
        }

        private async Task<SyncEntity?> PerformFieldMergeAsync(ConflictInfo conflict)
        {
            // This would contain sophisticated field-level merge logic
            // For now, return cloud version
            return JsonConvert.DeserializeObject<SyncEntity>(conflict.CloudData);
        }

        private string CalculatePackageChecksum(SyncPackage package)
        {
            var packageJson = JsonConvert.SerializeObject(package, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(packageJson));
            return Convert.ToBase64String(hashBytes);
        }

        private string CalculateEntityHash(SyncEntity entity)
        {
            var entityJson = JsonConvert.SerializeObject(entity, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(entityJson));
            return Convert.ToBase64String(hashBytes);
        }

        private bool IsValidEntity(SyncEntity entity)
        {
            return !string.IsNullOrEmpty(entity.EntityType) && 
                   entity.LocalId != Guid.Empty && 
                   !string.IsNullOrEmpty(entity.EntityData);
        }
    }
}
