using System.ComponentModel.DataAnnotations;

namespace ERP_AI.Desktop.Models
{
    public class AuditLog
    {
        public string Id { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string EventCategory { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string? UserEmail { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? ResourceType { get; set; }
        public string? ResourceId { get; set; }
        public string? Action { get; set; }
        public string? Details { get; set; }
        public string? FailureReason { get; set; }
        public bool Success { get; set; } = true;
        public DateTime Timestamp { get; set; }
        public string? SessionId { get; set; }
        public string? RequestId { get; set; }
    }

    public class AuditLogFilter
    {
        public string? EventType { get; set; }
        public string? EventCategory { get; set; }
        public string? UserId { get; set; }
        public string? UserEmail { get; set; }
        public string? IpAddress { get; set; }
        public string? ResourceType { get; set; }
        public string? Action { get; set; }
        public bool? Success { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageSize { get; set; } = 50;
        public int PageNumber { get; set; } = 1;
        public string? SortBy { get; set; } = "Timestamp";
        public bool SortDescending { get; set; } = true;
    }

    public class AuditReport
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public string GeneratedBy { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<AuditLog> Logs { get; set; } = new();
        public AuditStatistics Statistics { get; set; } = new();
        public string? FilePath { get; set; }
    }

    public class AuditStatistics
    {
        public int TotalEvents { get; set; }
        public int SuccessfulEvents { get; set; }
        public int FailedEvents { get; set; }
        public int LoginAttempts { get; set; }
        public int FailedLogins { get; set; }
        public int PasswordChanges { get; set; }
        public int AccountLockouts { get; set; }
        public int SecurityEvents { get; set; }
        public Dictionary<string, int> EventsByType { get; set; } = new();
        public Dictionary<string, int> EventsByUser { get; set; } = new();
        public Dictionary<string, int> EventsByIpAddress { get; set; } = new();
    }

    public class AuditSummary
    {
        public DateTime Date { get; set; }
        public int TotalEvents { get; set; }
        public int SuccessfulEvents { get; set; }
        public int FailedEvents { get; set; }
        public int UniqueUsers { get; set; }
        public int UniqueIpAddresses { get; set; }
        public Dictionary<string, int> TopEventTypes { get; set; } = new();
        public Dictionary<string, int> TopUsers { get; set; } = new();
    }

    public class SecurityEvent
    {
        public string Id { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string? UserEmail { get; set; }
        public string? IpAddress { get; set; }
        public string? Details { get; set; }
        public DateTime Timestamp { get; set; }
        public bool Resolved { get; set; } = false;
        public DateTime? ResolvedAt { get; set; }
        public string? ResolvedBy { get; set; }
    }

    public class AuditReportFilter
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string? EventType { get; set; }
        public string? EventCategory { get; set; }
        public string? UserId { get; set; }
        public string? IpAddress { get; set; }
        public bool? Success { get; set; }
        public string? Format { get; set; } = "PDF"; // PDF, Excel, CSV
    }

    public class AuditConfiguration
    {
        public bool IsEnabled { get; set; } = true;
        public bool LogAuthenticationEvents { get; set; } = true;
        public bool LogUserManagementEvents { get; set; } = true;
        public bool LogSystemEvents { get; set; } = true;
        public bool LogDataAccessEvents { get; set; } = true;
        public bool LogSecurityEvents { get; set; } = true;
        public int RetentionDays { get; set; } = 365;
        public bool IncludeIpAddress { get; set; } = true;
        public bool IncludeUserAgent { get; set; } = true;
        public bool IncludeSessionId { get; set; } = true;
        public List<string> ExcludedEventTypes { get; set; } = new();
        public List<string> ExcludedUsers { get; set; } = new();
        public int MaxLogSize { get; set; } = 1000000; // 1MB
        public bool CompressOldLogs { get; set; } = true;
    }

    // Event Types
    public static class AuditEventTypes
    {
        // Authentication Events
        public const string LoginAttempt = "LOGIN_ATTEMPT";
        public const string LoginSuccess = "LOGIN_SUCCESS";
        public const string LoginFailure = "LOGIN_FAILURE";
        public const string Logout = "LOGOUT";
        public const string PasswordChange = "PASSWORD_CHANGE";
        public const string PasswordChangeSuccess = "PASSWORD_CHANGE_SUCCESS";
        public const string PasswordChangeFailure = "PASSWORD_CHANGE_FAILURE";
        public const string PasswordResetRequest = "PASSWORD_RESET_REQUEST";
        public const string PasswordResetComplete = "PASSWORD_RESET_COMPLETE";
        public const string AccountLockout = "ACCOUNT_LOCKOUT";
        public const string AccountUnlock = "ACCOUNT_UNLOCK";
        public const string TwoFactorEnabled = "TWO_FACTOR_ENABLED";
        public const string TwoFactorDisabled = "TWO_FACTOR_DISABLED";

        // User Management Events
        public const string UserCreated = "USER_CREATED";
        public const string UserUpdated = "USER_UPDATED";
        public const string UserDeleted = "USER_DELETED";
        public const string UserActivated = "USER_ACTIVATED";
        public const string UserDeactivated = "USER_DEACTIVATED";
        public const string RoleAssigned = "ROLE_ASSIGNED";
        public const string RoleRemoved = "ROLE_REMOVED";
        public const string PermissionGranted = "PERMISSION_GRANTED";
        public const string PermissionRevoked = "PERMISSION_REVOKED";

        // System Events
        public const string SystemStartup = "SYSTEM_STARTUP";
        public const string SystemShutdown = "SYSTEM_SHUTDOWN";
        public const string ConfigurationChanged = "CONFIGURATION_CHANGED";
        public const string DatabaseBackup = "DATABASE_BACKUP";
        public const string DatabaseRestore = "DATABASE_RESTORE";
        public const string DataExport = "DATA_EXPORT";
        public const string DataImport = "DATA_IMPORT";

        // Security Events
        public const string SuspiciousActivity = "SUSPICIOUS_ACTIVITY";
        public const string UnauthorizedAccess = "UNAUTHORIZED_ACCESS";
        public const string DataBreach = "DATA_BREACH";
        public const string SecurityScan = "SECURITY_SCAN";
        public const string VulnerabilityDetected = "VULNERABILITY_DETECTED";
        public const string SecurityPolicyViolation = "SECURITY_POLICY_VIOLATION";
    }

    // Event Categories
    public static class AuditEventCategories
    {
        public const string Authentication = "Authentication";
        public const string UserManagement = "User Management";
        public const string System = "System";
        public const string Security = "Security";
        public const string DataAccess = "Data Access";
        public const string Configuration = "Configuration";
    }

    // Severity Levels
    public static class SecuritySeverity
    {
        public const string Low = "Low";
        public const string Medium = "Medium";
        public const string High = "High";
        public const string Critical = "Critical";
    }
}
