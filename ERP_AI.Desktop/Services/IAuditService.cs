using ERP_AI.Desktop.Models;

namespace ERP_AI.Desktop.Services
{
    public interface IAuditService
    {
        // Audit Log Management
        Task<AuditLog> CreateAuditLogAsync(AuditLog auditLog);
        Task<List<AuditLog>> GetAuditLogsAsync(AuditLogFilter? filter = null);
        Task<AuditLog?> GetAuditLogByIdAsync(string auditLogId);
        Task<bool> DeleteAuditLogAsync(string auditLogId);
        
        // Authentication Events
        Task LogLoginAttemptAsync(string userId, string email, bool success, string? failureReason = null, string? ipAddress = null);
        Task LogLogoutAsync(string userId, string email, string? ipAddress = null);
        Task LogPasswordChangeAsync(string userId, string email, bool success, string? failureReason = null);
        Task LogAccountLockoutAsync(string userId, string email, string reason, string? ipAddress = null);
        Task LogAccountUnlockAsync(string userId, string email, string? ipAddress = null);
        Task LogPasswordResetRequestAsync(string userId, string email, bool success, string? failureReason = null);
        Task LogPasswordResetCompleteAsync(string userId, string email, bool success, string? failureReason = null);
        
        // User Management Events
        Task LogUserCreationAsync(string userId, string email, string createdBy);
        Task LogUserUpdateAsync(string userId, string email, string updatedBy, List<string> changedFields);
        Task LogUserDeletionAsync(string userId, string email, string deletedBy);
        Task LogRoleAssignmentAsync(string userId, string email, string roleName, string assignedBy);
        Task LogRoleRemovalAsync(string userId, string email, string roleName, string removedBy);
        
        // System Events
        Task LogSystemEventAsync(string eventType, string description, string? userId = null, string? details = null);
        Task LogSecurityEventAsync(string eventType, string description, string? userId = null, string? ipAddress = null, string? details = null);
        Task LogDataAccessAsync(string userId, string resourceType, string resourceId, string action, string? details = null);
        
        // Audit Reports
        Task<AuditReport> GenerateAuditReportAsync(AuditReportFilter filter);
        Task<List<AuditSummary>> GetAuditSummaryAsync(DateTime fromDate, DateTime toDate);
        Task<List<SecurityEvent>> GetSecurityEventsAsync(DateTime fromDate, DateTime toDate);
        
        // Audit Configuration
        Task<AuditConfiguration> GetAuditConfigurationAsync();
        Task UpdateAuditConfigurationAsync(AuditConfiguration configuration);
        Task<bool> IsAuditEnabledAsync();
        Task EnableAuditAsync();
        Task DisableAuditAsync();
    }
}
