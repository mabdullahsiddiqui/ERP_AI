using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP_AI.CloudAPI.Models;
using ERP_AI.CloudAPI.Services;
using ERP_AI.Data;
using System.Security.Claims;

namespace ERP_AI.CloudAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class SyncController : ControllerBase
    {
        private readonly ISyncProcessingService _syncService;
        private readonly ILogger<SyncController> _logger;

        public SyncController(
            ISyncProcessingService syncService,
            ILogger<SyncController> logger)
        {
            _syncService = syncService;
            _logger = logger;
        }

        /// <summary>
        /// Upload sync package with changes from desktop client
        /// </summary>
        [HttpPost("upload")]
        public async Task<ActionResult<ApiResponse<SyncUploadResponse>>> UploadChanges(
            [FromBody] SyncUploadRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var companyId = GetCurrentCompanyId();

                // Validate company access
                if (request.CompanyId != companyId)
                {
                    return Forbid("Access denied to company data");
                }

                _logger.LogInformation("Processing sync upload for company {CompanyId}, user {UserId}", 
                    companyId, userId);

                var response = await _syncService.ProcessUploadAsync(request, userId);
                
                _logger.LogInformation("Sync upload processed: {SuccessfulEntities}/{TotalEntities} successful", 
                    response.SuccessfulEntities, response.ProcessedEntities);

                return Ok(ApiResponse.SuccessResult(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing sync upload");
                return StatusCode(500, ApiResponse.ErrorResult<SyncUploadResponse>(
                    "Internal server error during sync upload", "SYNC_UPLOAD_ERROR"));
            }
        }

        /// <summary>
        /// Download sync package with changes from server
        /// </summary>
        [HttpPost("download")]
        public async Task<ActionResult<ApiResponse<SyncDownloadResponse>>> DownloadChanges(
            [FromBody] SyncDownloadRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var companyId = GetCurrentCompanyId();

                // Validate company access
                if (request.CompanyId != companyId)
                {
                    return Forbid("Access denied to company data");
                }

                _logger.LogInformation("Processing sync download for company {CompanyId}, user {UserId}", 
                    companyId, userId);

                var response = await _syncService.ProcessDownloadAsync(request, userId);
                
                _logger.LogInformation("Sync download processed: {EntityCount} entities returned", 
                    response.Package.ChangeSets.Sum(cs => cs.Entities.Count));

                return Ok(ApiResponse.SuccessResult(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing sync download");
                return StatusCode(500, ApiResponse.ErrorResult<SyncDownloadResponse>(
                    "Internal server error during sync download", "SYNC_DOWNLOAD_ERROR"));
            }
        }

        /// <summary>
        /// Resolve conflicts identified during sync
        /// </summary>
        [HttpPost("conflicts/resolve")]
        public async Task<ActionResult<ApiResponse<ConflictResolutionResponse>>> ResolveConflict(
            [FromBody] ConflictResolutionRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var companyId = GetCurrentCompanyId();

                // Validate company access
                if (request.CompanyId != companyId)
                {
                    return Forbid("Access denied to company data");
                }

                _logger.LogInformation("Resolving conflict {ConflictId} for company {CompanyId}", 
                    request.ConflictId, companyId);

                var response = await _syncService.ResolveConflictAsync(request, userId);
                
                _logger.LogInformation("Conflict {ConflictId} resolved successfully", request.ConflictId);

                return Ok(ApiResponse.SuccessResult(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving conflict {ConflictId}", request.ConflictId);
                return StatusCode(500, ApiResponse.ErrorResult<ConflictResolutionResponse>(
                    "Internal server error during conflict resolution", "CONFLICT_RESOLUTION_ERROR"));
            }
        }

        /// <summary>
        /// Get sync status for company
        /// </summary>
        [HttpPost("status")]
        public async Task<ActionResult<ApiResponse<SyncStatusResponse>>> GetSyncStatus(
            [FromBody] SyncStatusRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var companyId = GetCurrentCompanyId();

                // Validate company access
                if (request.CompanyId != companyId)
                {
                    return Forbid("Access denied to company data");
                }

                var response = await _syncService.GetSyncStatusAsync(request, userId);

                return Ok(ApiResponse.SuccessResult(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sync status for company {CompanyId}", request.CompanyId);
                return StatusCode(500, ApiResponse.ErrorResult<SyncStatusResponse>(
                    "Internal server error getting sync status", "SYNC_STATUS_ERROR"));
            }
        }

        /// <summary>
        /// Get list of pending conflicts
        /// </summary>
        [HttpGet("conflicts")]
        public async Task<ActionResult<ApiResponse<List<ConflictInfo>>>> GetPendingConflicts(
            [FromQuery] string? entityType = null)
        {
            try
            {
                var companyId = GetCurrentCompanyId();
                var conflicts = await _syncService.GetPendingConflictsAsync(companyId, entityType);

                return Ok(ApiResponse.SuccessResult(conflicts));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending conflicts for company {CompanyId}", GetCurrentCompanyId());
                return StatusCode(500, ApiResponse.ErrorResult<List<ConflictInfo>>(
                    "Internal server error getting conflicts", "GET_CONFLICTS_ERROR"));
            }
        }

        /// <summary>
        /// Process batch sync operations
        /// </summary>
        [HttpPost("batch")]
        public async Task<ActionResult<ApiResponse<BatchSyncResponse>>> ProcessBatchSync(
            [FromBody] BatchSyncRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var companyId = GetCurrentCompanyId();

                // Validate company access
                if (request.CompanyId != companyId)
                {
                    return Forbid("Access denied to company data");
                }

                _logger.LogInformation("Processing batch sync with {PackageCount} packages for company {CompanyId}", 
                    request.Packages.Count, companyId);

                var response = await _syncService.ProcessBatchSyncAsync(request, userId);
                
                _logger.LogInformation("Batch sync processed: {SuccessfulPackages}/{TotalPackages} successful", 
                    response.SuccessfulPackages, response.TotalPackages);

                return Ok(ApiResponse.SuccessResult(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing batch sync");
                return StatusCode(500, ApiResponse.ErrorResult<BatchSyncResponse>(
                    "Internal server error during batch sync", "BATCH_SYNC_ERROR"));
            }
        }

        /// <summary>
        /// Get sync history
        /// </summary>
        [HttpPost("history")]
        public async Task<ActionResult<ApiResponse<SyncHistoryResponse>>> GetSyncHistory(
            [FromBody] SyncHistoryRequest request)
        {
            try
            {
                var companyId = GetCurrentCompanyId();

                // Validate company access
                if (request.CompanyId != companyId)
                {
                    return Forbid("Access denied to company data");
                }

                var response = await _syncService.GetSyncHistoryAsync(request);

                return Ok(ApiResponse.SuccessResult(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sync history for company {CompanyId}", request.CompanyId);
                return StatusCode(500, ApiResponse.ErrorResult<SyncHistoryResponse>(
                    "Internal server error getting sync history", "SYNC_HISTORY_ERROR"));
            }
        }

        /// <summary>
        /// Health check endpoint for sync services
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<object>>> HealthCheck()
        {
            try
            {
                var health = await _syncService.GetHealthStatusAsync();
                
                var healthData = new
                {
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    Version = "1.0",
                    Services = health
                };

                return Ok(ApiResponse.SuccessResult(healthData));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(500, ApiResponse.ErrorResult<object>(
                    "Health check failed", "HEALTH_CHECK_ERROR"));
            }
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }

        private string GetCurrentCompanyId()
        {
            return User.FindFirst("CompanyId")?.Value ?? string.Empty;
        }
    }
}
