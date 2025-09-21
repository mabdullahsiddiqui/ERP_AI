using System.Text.RegularExpressions;
using ERP_AI.Desktop.Models;

namespace ERP_AI.Desktop.Services
{
    public class MockPasswordPolicyService : IPasswordPolicyService
    {
        private readonly PasswordPolicySettings _settings;
        private readonly Dictionary<string, List<PasswordHistory>> _passwordHistory;
        private readonly Dictionary<string, AccountLockout> _accountLockouts;
        private readonly List<string> _commonPasswords;

        public MockPasswordPolicyService()
        {
            _settings = new PasswordPolicySettings();
            _passwordHistory = new Dictionary<string, List<PasswordHistory>>();
            _accountLockouts = new Dictionary<string, AccountLockout>();
            _commonPasswords = InitializeCommonPasswords();
        }

        #region Password Validation

        public PasswordValidationResult ValidatePassword(string password, string? confirmPassword = null)
        {
            var result = new PasswordValidationResult();

            // Check if password is null or empty
            if (string.IsNullOrWhiteSpace(password))
            {
                result.Errors.Add("Password is required.");
                return result;
            }

            // Check minimum length
            if (password.Length < _settings.Requirements.MinimumLength)
            {
                result.Errors.Add($"Password must be at least {_settings.Requirements.MinimumLength} characters long.");
            }

            // Check maximum length
            if (password.Length > _settings.Requirements.MaximumLength)
            {
                result.Errors.Add($"Password must be no more than {_settings.Requirements.MaximumLength} characters long.");
            }

            // Check uppercase requirement
            if (_settings.Requirements.RequireUppercase && !ContainsUppercase(password))
            {
                result.Errors.Add("Password must contain at least one uppercase letter.");
            }

            // Check lowercase requirement
            if (_settings.Requirements.RequireLowercase && !ContainsLowercase(password))
            {
                result.Errors.Add("Password must contain at least one lowercase letter.");
            }

            // Check digit requirement
            if (_settings.Requirements.RequireDigit && !ContainsDigit(password))
            {
                result.Errors.Add("Password must contain at least one digit.");
            }

            // Check special character requirement
            if (_settings.Requirements.RequireSpecialCharacter && !ContainsSpecialCharacter(password))
            {
                result.Errors.Add("Password must contain at least one special character.");
            }

            // Check for consecutive characters
            if (HasConsecutiveCharacters(password, _settings.Requirements.MaximumConsecutiveCharacters))
            {
                result.Warnings.Add($"Password should not contain more than {_settings.Requirements.MaximumConsecutiveCharacters} consecutive identical characters.");
            }

            // Check for common passwords
            if (_settings.Requirements.PreventCommonPasswords && IsNotCommonPassword(password))
            {
                result.Warnings.Add("This password is commonly used and may be easily guessed.");
            }

            // Check password confirmation
            if (!string.IsNullOrEmpty(confirmPassword) && password != confirmPassword)
            {
                result.Errors.Add("Password and confirmation password do not match.");
            }

            // Calculate password strength
            result.Strength = CalculatePasswordStrength(password);
            result.Score = CalculatePasswordScore(password);
            result.IsValid = result.Errors.Count == 0;

            return result;
        }

        public PasswordValidationResult ValidatePasswordChange(string currentPassword, string newPassword, string confirmPassword)
        {
            var result = ValidatePassword(newPassword, confirmPassword);

            // Check if new password is different from current password
            if (currentPassword == newPassword)
            {
                result.Errors.Add("New password must be different from current password.");
            }

            return result;
        }

        #endregion

        #region Password Requirements

        public PasswordRequirements GetPasswordRequirements()
        {
            return _settings.Requirements;
        }

        public bool MeetsMinimumLength(string password)
        {
            return password.Length >= _settings.Requirements.MinimumLength;
        }

        public bool ContainsUppercase(string password)
        {
            return password.Any(char.IsUpper);
        }

        public bool ContainsLowercase(string password)
        {
            return password.Any(char.IsLower);
        }

        public bool ContainsDigit(string password)
        {
            return password.Any(char.IsDigit);
        }

        public bool ContainsSpecialCharacter(string password)
        {
            var specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";
            return password.Any(c => specialChars.Contains(c));
        }

        public bool IsNotCommonPassword(string password)
        {
            return !_commonPasswords.Contains(password.ToLower());
        }

        public bool IsNotUserInformation(string password, string firstName, string lastName, string email)
        {
            var passwordLower = password.ToLower();
            var firstNameLower = firstName.ToLower();
            var lastNameLower = lastName.ToLower();
            var emailLower = email.ToLower();

            return !passwordLower.Contains(firstNameLower) &&
                   !passwordLower.Contains(lastNameLower) &&
                   !passwordLower.Contains(emailLower.Split('@')[0]);
        }

        #endregion

        #region Password History

        public async Task<bool> IsPasswordInHistoryAsync(string userId, string password)
        {
            await Task.Delay(100); // Simulate API call

            if (!_passwordHistory.ContainsKey(userId))
                return false;

            var hashedPassword = HashPassword(password);
            return _passwordHistory[userId].Any(h => h.PasswordHash == hashedPassword);
        }

        public async Task AddPasswordToHistoryAsync(string userId, string password)
        {
            await Task.Delay(100); // Simulate API call

            if (!_passwordHistory.ContainsKey(userId))
                _passwordHistory[userId] = new List<PasswordHistory>();

            var hashedPassword = HashPassword(password);
            var historyEntry = new PasswordHistory
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                PasswordHash = hashedPassword,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddDays(_settings.Requirements.PasswordExpirationDays)
            };

            _passwordHistory[userId].Add(historyEntry);

            // Keep only the most recent passwords based on history count
            if (_passwordHistory[userId].Count > _settings.Requirements.PasswordHistoryCount)
            {
                _passwordHistory[userId] = _passwordHistory[userId]
                    .OrderByDescending(h => h.CreatedAt)
                    .Take(_settings.Requirements.PasswordHistoryCount)
                    .ToList();
            }
        }

        public async Task ClearPasswordHistoryAsync(string userId)
        {
            await Task.Delay(100); // Simulate API call

            if (_passwordHistory.ContainsKey(userId))
            {
                _passwordHistory[userId].Clear();
            }
        }

        #endregion

        #region Password Expiration

        public async Task<bool> IsPasswordExpiredAsync(string userId)
        {
            await Task.Delay(100); // Simulate API call

            if (!_passwordHistory.ContainsKey(userId) || !_passwordHistory[userId].Any())
                return false;

            var latestPassword = _passwordHistory[userId]
                .OrderByDescending(h => h.CreatedAt)
                .First();

            return DateTime.Now > latestPassword.ExpiresAt;
        }

        public async Task<DateTime?> GetPasswordExpirationDateAsync(string userId)
        {
            await Task.Delay(100); // Simulate API call

            if (!_passwordHistory.ContainsKey(userId) || !_passwordHistory[userId].Any())
                return null;

            var latestPassword = _passwordHistory[userId]
                .OrderByDescending(h => h.CreatedAt)
                .First();

            return latestPassword.ExpiresAt;
        }

        public async Task SetPasswordExpirationAsync(string userId, DateTime expirationDate)
        {
            await Task.Delay(100); // Simulate API call

            if (!_passwordHistory.ContainsKey(userId) || !_passwordHistory[userId].Any())
                return;

            var latestPassword = _passwordHistory[userId]
                .OrderByDescending(h => h.CreatedAt)
                .First();

            latestPassword.ExpiresAt = expirationDate;
        }

        #endregion

        #region Account Lockout

        public async Task<bool> IsAccountLockedAsync(string userId)
        {
            await Task.Delay(100); // Simulate API call

            if (!_accountLockouts.ContainsKey(userId))
                return false;

            var lockout = _accountLockouts[userId];
            
            if (lockout.IsLocked && lockout.LockedUntil.HasValue)
            {
                if (DateTime.Now > lockout.LockedUntil.Value)
                {
                    // Lockout has expired
                    lockout.IsLocked = false;
                    lockout.LockedUntil = null;
                    lockout.FailedAttempts = 0;
                    return false;
                }
            }

            return lockout.IsLocked;
        }

        public async Task<int> GetFailedAttemptsAsync(string userId)
        {
            await Task.Delay(100); // Simulate API call

            if (!_accountLockouts.ContainsKey(userId))
                return 0;

            return _accountLockouts[userId].FailedAttempts;
        }

        public async Task IncrementFailedAttemptsAsync(string userId)
        {
            await Task.Delay(100); // Simulate API call

            if (!_accountLockouts.ContainsKey(userId))
            {
                _accountLockouts[userId] = new AccountLockout
                {
                    UserId = userId,
                    IsLocked = false,
                    FailedAttempts = 0
                };
            }

            var lockout = _accountLockouts[userId];
            lockout.FailedAttempts++;
            lockout.LastFailedAttempt = DateTime.Now;

            // Check if account should be locked
            if (lockout.FailedAttempts >= _settings.Requirements.MaximumFailedAttempts)
            {
                await LockAccountAsync(userId);
            }
        }

        public async Task ResetFailedAttemptsAsync(string userId)
        {
            await Task.Delay(100); // Simulate API call

            if (_accountLockouts.ContainsKey(userId))
            {
                _accountLockouts[userId].FailedAttempts = 0;
                _accountLockouts[userId].LastFailedAttempt = null;
            }
        }

        public async Task LockAccountAsync(string userId, TimeSpan? lockoutDuration = null)
        {
            await Task.Delay(100); // Simulate API call

            if (!_accountLockouts.ContainsKey(userId))
            {
                _accountLockouts[userId] = new AccountLockout
                {
                    UserId = userId,
                    IsLocked = false,
                    FailedAttempts = 0
                };
            }

            var lockout = _accountLockouts[userId];
            lockout.IsLocked = true;
            lockout.LockedUntil = DateTime.Now.Add(lockoutDuration ?? TimeSpan.FromMinutes(_settings.Requirements.LockoutDurationMinutes));
        }

        public async Task UnlockAccountAsync(string userId)
        {
            await Task.Delay(100); // Simulate API call

            if (_accountLockouts.ContainsKey(userId))
            {
                var lockout = _accountLockouts[userId];
                lockout.IsLocked = false;
                lockout.LockedUntil = null;
                lockout.FailedAttempts = 0;
            }
        }

        #endregion

        #region Private Helper Methods

        private PasswordStrength CalculatePasswordStrength(string password)
        {
            int score = CalculatePasswordScore(password);

            return score switch
            {
                < 20 => PasswordStrength.VeryWeak,
                < 40 => PasswordStrength.Weak,
                < 60 => PasswordStrength.Fair,
                < 80 => PasswordStrength.Good,
                < 100 => PasswordStrength.Strong,
                _ => PasswordStrength.VeryStrong
            };
        }

        private int CalculatePasswordScore(string password)
        {
            int score = 0;

            // Length score
            score += Math.Min(password.Length * 2, 20);

            // Character variety score
            if (ContainsUppercase(password)) score += 10;
            if (ContainsLowercase(password)) score += 10;
            if (ContainsDigit(password)) score += 10;
            if (ContainsSpecialCharacter(password)) score += 15;

            // Complexity score
            if (password.Length >= 12) score += 10;
            if (password.Length >= 16) score += 5;

            // Penalty for common patterns
            if (HasConsecutiveCharacters(password, 3)) score -= 5;
            if (IsNotCommonPassword(password)) score -= 10;

            return Math.Max(0, Math.Min(100, score));
        }

        private bool HasConsecutiveCharacters(string password, int maxConsecutive)
        {
            int consecutiveCount = 1;
            char previousChar = password[0];

            for (int i = 1; i < password.Length; i++)
            {
                if (password[i] == previousChar)
                {
                    consecutiveCount++;
                    if (consecutiveCount > maxConsecutive)
                        return true;
                }
                else
                {
                    consecutiveCount = 1;
                    previousChar = password[i];
                }
            }

            return false;
        }

        private string HashPassword(string password)
        {
            // In a real implementation, use proper password hashing like BCrypt
            // This is just for demonstration
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
        }

        private List<string> InitializeCommonPasswords()
        {
            return new List<string>
            {
                "password", "123456", "123456789", "12345678", "12345", "1234567",
                "password123", "admin", "letmein", "welcome", "monkey", "1234567890",
                "abc123", "111111", "dragon", "master", "hello", "freedom",
                "whatever", "qazwsx", "trustno1", "jordan23", "harley", "ranger",
                "hunter", "buster", "soccer", "hockey", "killer", "george",
                "sexy", "andrew", "charlie", "superman", "asshole", "fuckyou",
                "dallas", "jessica", "panties", "pepper", "1234", "696969",
                "killer", "trustno1", "jordan", "jennifer", "zxcvbn", "asdfgh",
                "hunter", "buster", "soccer", "hockey", "killer", "george",
                "sexy", "andrew", "charlie", "superman", "asshole", "fuckyou"
            };
        }

        #endregion
    }
}
