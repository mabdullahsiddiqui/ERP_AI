using ERP_AI.Desktop.Models;

namespace ERP_AI.Desktop.Services
{
    public interface IAuthenticationService
    {
        // Authentication Events
        event EventHandler<AuthState>? AuthenticationStateChanged;
        event EventHandler<UserInfo>? UserLoggedIn;
        event EventHandler? UserLoggedOut;

        // Current State
        AuthState CurrentState { get; }
        bool IsAuthenticated { get; }
        UserInfo? CurrentUser { get; }
        CompanyInfo? CurrentCompany { get; }

        // Authentication Methods
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<LoginResponse> RegisterAsync(RegisterRequest request);
        Task<bool> LogoutAsync();
        Task<bool> RefreshTokenAsync();
        Task<bool> ValidateTokenAsync();

        // User Management
        Task<UserInfo> GetUserInfoAsync();
        Task<CompanyInfo> GetCompanyInfoAsync();
        Task<bool> UpdateProfileAsync(ProfileUpdateRequest request);
        Task<bool> ChangePasswordAsync(PasswordChangeRequest request);
        Task<bool> UpdateCompanyAsync(CompanyUpdateRequest request);
        
        // Enhanced User Profile Management
        Task<UserProfileUpdateResponse> UpdateUserProfileAsync(UserProfileUpdateRequest request);
        Task<ChangePasswordResponse> ChangeUserPasswordAsync(ChangePasswordRequest request);
        Task<UserInfo?> GetCurrentUserAsync();
        Task<bool> DeleteAccountAsync(string password);

        // Session Management
        Task<bool> SaveSessionAsync();
        Task<bool> LoadSessionAsync();
        Task<bool> ClearSessionAsync();
        Task<bool> IsSessionValidAsync();

        // Security
        Task<bool> ValidateApiKeyAsync(string apiKey);
        Task<string> GenerateApiKeyAsync(string name, DateTime? expiresAt = null);
        Task<List<ApiKeyResponse>> GetApiKeysAsync();
        Task<bool> RevokeApiKeyAsync(string keyId);

        // Utility Methods
        Task<bool> CheckConnectionAsync();
        Task<bool> IsOnlineAsync();
        void SetOfflineMode(bool isOffline);
    }

    public class ApiKeyResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public List<string> Permissions { get; set; } = new();
        public bool IsActive { get; set; } = true;
    }
}

