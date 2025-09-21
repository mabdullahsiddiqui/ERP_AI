using System.ComponentModel.DataAnnotations;

namespace ERP_AI.Desktop.Models
{
    public class PasswordValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public PasswordStrength Strength { get; set; }
        public int Score { get; set; }
    }

    public class PasswordRequirements
    {
        public int MinimumLength { get; set; } = 8;
        public int MaximumLength { get; set; } = 128;
        public bool RequireUppercase { get; set; } = true;
        public bool RequireLowercase { get; set; } = true;
        public bool RequireDigit { get; set; } = true;
        public bool RequireSpecialCharacter { get; set; } = true;
        public int MinimumSpecialCharacters { get; set; } = 1;
        public bool PreventCommonPasswords { get; set; } = true;
        public bool PreventUserInformation { get; set; } = true;
        public int MaximumConsecutiveCharacters { get; set; } = 3;
        public int PasswordHistoryCount { get; set; } = 5;
        public int PasswordExpirationDays { get; set; } = 90;
        public int MaximumFailedAttempts { get; set; } = 5;
        public int LockoutDurationMinutes { get; set; } = 30;
    }

    public enum PasswordStrength
    {
        VeryWeak = 0,
        Weak = 1,
        Fair = 2,
        Good = 3,
        Strong = 4,
        VeryStrong = 5
    }

    public class PasswordHistory
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class AccountLockout
    {
        public string UserId { get; set; } = string.Empty;
        public bool IsLocked { get; set; }
        public int FailedAttempts { get; set; }
        public DateTime? LockedUntil { get; set; }
        public DateTime? LastFailedAttempt { get; set; }
    }

    public class PasswordPolicySettings
    {
        public PasswordRequirements Requirements { get; set; } = new();
        public bool EnablePasswordHistory { get; set; } = true;
        public bool EnablePasswordExpiration { get; set; } = true;
        public bool EnableAccountLockout { get; set; } = true;
        public bool EnableTwoFactorAuthentication { get; set; } = false;
        public bool RequirePasswordChangeOnFirstLogin { get; set; } = true;
        public bool NotifyPasswordExpiration { get; set; } = true;
        public int PasswordExpirationWarningDays { get; set; } = 7;
    }
}
