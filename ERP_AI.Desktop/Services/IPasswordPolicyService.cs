using ERP_AI.Desktop.Models;

namespace ERP_AI.Desktop.Services
{
    public interface IPasswordPolicyService
    {
        // Password Validation
        PasswordValidationResult ValidatePassword(string password, string? confirmPassword = null);
        PasswordValidationResult ValidatePasswordChange(string currentPassword, string newPassword, string confirmPassword);
        
        // Password Requirements
        PasswordRequirements GetPasswordRequirements();
        bool MeetsMinimumLength(string password);
        bool ContainsUppercase(string password);
        bool ContainsLowercase(string password);
        bool ContainsDigit(string password);
        bool ContainsSpecialCharacter(string password);
        bool IsNotCommonPassword(string password);
        bool IsNotUserInformation(string password, string firstName, string lastName, string email);
        
        // Password History
        Task<bool> IsPasswordInHistoryAsync(string userId, string password);
        Task AddPasswordToHistoryAsync(string userId, string password);
        Task ClearPasswordHistoryAsync(string userId);
        
        // Password Expiration
        Task<bool> IsPasswordExpiredAsync(string userId);
        Task<DateTime?> GetPasswordExpirationDateAsync(string userId);
        Task SetPasswordExpirationAsync(string userId, DateTime expirationDate);
        
        // Account Lockout
        Task<bool> IsAccountLockedAsync(string userId);
        Task<int> GetFailedAttemptsAsync(string userId);
        Task IncrementFailedAttemptsAsync(string userId);
        Task ResetFailedAttemptsAsync(string userId);
        Task LockAccountAsync(string userId, TimeSpan? lockoutDuration = null);
        Task UnlockAccountAsync(string userId);
    }
}
