using ERP_AI.CloudAPI.Models;

namespace ERP_AI.CloudAPI.Services
{
    public interface IAuthService
    {
        /// <summary>
        /// Authenticate user with email and password
        /// </summary>
        Task<LoginResponse> LoginAsync(LoginRequest request);

        /// <summary>
        /// Register new user and company
        /// </summary>
        Task<LoginResponse> RegisterAsync(RegisterRequest request);

        /// <summary>
        /// Refresh JWT token
        /// </summary>
        Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request);

        /// <summary>
        /// Logout user and invalidate tokens
        /// </summary>
        Task LogoutAsync(string userId);

        /// <summary>
        /// Create API key for desktop application
        /// </summary>
        Task<ApiKeyResponse> CreateApiKeyAsync(ApiKeyRequest request, string userId, string companyId);

        /// <summary>
        /// Get list of API keys for user
        /// </summary>
        Task<List<ApiKeyResponse>> GetApiKeysAsync(string userId);

        /// <summary>
        /// Revoke API key
        /// </summary>
        Task RevokeApiKeyAsync(string keyId, string userId);

        /// <summary>
        /// Get user information
        /// </summary>
        Task<UserInfo> GetUserInfoAsync(string userId);

        /// <summary>
        /// Get company information
        /// </summary>
        Task<CompanyInfo> GetCompanyInfoAsync(string companyId);

        /// <summary>
        /// Validate API key
        /// </summary>
        Task<bool> ValidateApiKeyAsync(string apiKey, string companyId);

        /// <summary>
        /// Generate JWT token for user
        /// </summary>
        Task<string> GenerateJwtTokenAsync(UserInfo user);

        /// <summary>
        /// Generate refresh token
        /// </summary>
        Task<string> GenerateRefreshTokenAsync(string userId);

        /// <summary>
        /// Validate and decode JWT token
        /// </summary>
        Task<UserInfo?> ValidateJwtTokenAsync(string token);

        /// <summary>
        /// Get all users for a company
        /// </summary>
        Task<List<UserInfo>> GetUsersAsync(string companyId, int page = 1, int pageSize = 10);

        /// <summary>
        /// Get user by ID
        /// </summary>
        Task<UserInfo?> GetUserByIdAsync(string userId);

        /// <summary>
        /// Create new user
        /// </summary>
        Task<UserInfo> CreateUserAsync(RegisterRequest request, string companyId);

        /// <summary>
        /// Update user information
        /// </summary>
        Task<UserInfo> UpdateUserAsync(string userId, RegisterRequest request);

        /// <summary>
        /// Delete user
        /// </summary>
        Task<bool> DeleteUserAsync(string userId);

        /// <summary>
        /// Reset user password
        /// </summary>
        Task<bool> ResetPasswordAsync(string userId, string newPassword);

        /// <summary>
        /// Get audit logs
        /// </summary>
        Task<List<AuditLog>> GetAuditLogsAsync(string? userId = null, string? eventType = null, DateTime? fromDate = null, DateTime? toDate = null);
    }
}
