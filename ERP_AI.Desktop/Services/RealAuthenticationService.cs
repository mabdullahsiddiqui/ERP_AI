using System.Text;
using System.Text.Json;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using ERP_AI.Desktop.Models;

namespace ERP_AI.Desktop.Services
{
    public class RealAuthenticationService : IAuthenticationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RealAuthenticationService> _logger;
        private readonly IConfiguration _configuration;

        public RealAuthenticationService(HttpClient httpClient, ILogger<RealAuthenticationService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
            
            var baseUrl = _configuration["CloudApi:BaseUrl"] ?? "https://localhost:7001";
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        // Events
        public event EventHandler<AuthState>? AuthenticationStateChanged;
        public event EventHandler<UserInfo>? UserLoggedIn;
        public event EventHandler? UserLoggedOut;

        // Current State
        public AuthState CurrentState { get; private set; } = new();
        public bool IsAuthenticated => CurrentState.IsAuthenticated;
        public UserInfo? CurrentUser => CurrentState.CurrentUser;
        public CompanyInfo? CurrentCompany => CurrentState.CurrentCompany;

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Attempting login for user: {Email}", request.Email);

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/auth/login", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (loginResponse?.Success == true && loginResponse.User != null)
                    {
                        CurrentState = new AuthState
                        {
                            IsAuthenticated = true,
                            CurrentUser = loginResponse.User,
                            CurrentCompany = loginResponse.Company,
                            AccessToken = loginResponse.Token,
                            RefreshToken = loginResponse.RefreshToken,
                            TokenExpiresAt = loginResponse.ExpiresAt,
                            IsOnline = true,
                            LastActivity = DateTime.Now
                        };

                        // Set authorization header for future requests
                        _httpClient.DefaultRequestHeaders.Authorization = 
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);

                        AuthenticationStateChanged?.Invoke(this, CurrentState);
                        UserLoggedIn?.Invoke(this, loginResponse.User);

                        _logger.LogInformation("Login successful for user: {Email}", request.Email);
                    }

                    return loginResponse ?? new LoginResponse { Success = false, ErrorMessage = "Invalid response" };
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);
                    var errorMessage = errorResponse?.ContainsKey("message") == true ? 
                        errorResponse["message"].ToString() : "Login failed";

                    _logger.LogWarning("Login failed for user: {Email}, Error: {Error}", request.Email, errorMessage);
                    return new LoginResponse { Success = false, ErrorMessage = errorMessage };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {Email}", request.Email);
                return new LoginResponse
                {
                    Success = false,
                    ErrorMessage = "An error occurred during login. Please check your connection and try again."
                };
            }
        }

        public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                _logger.LogInformation("Attempting registration for user: {Email}", request.Email);

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/auth/register", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var registerResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (registerResponse?.Success == true && registerResponse.User != null)
                    {
                        CurrentState = new AuthState
                        {
                            IsAuthenticated = true,
                            CurrentUser = registerResponse.User,
                            CurrentCompany = registerResponse.Company,
                            AccessToken = registerResponse.Token,
                            RefreshToken = registerResponse.RefreshToken,
                            TokenExpiresAt = registerResponse.ExpiresAt,
                            IsOnline = true,
                            LastActivity = DateTime.Now
                        };

                        _httpClient.DefaultRequestHeaders.Authorization = 
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", registerResponse.Token);

                        AuthenticationStateChanged?.Invoke(this, CurrentState);
                        UserLoggedIn?.Invoke(this, registerResponse.User);

                        _logger.LogInformation("Registration successful for user: {Email}", request.Email);
                    }

                    return registerResponse ?? new LoginResponse { Success = false, ErrorMessage = "Invalid response" };
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);
                    var errorMessage = errorResponse?.ContainsKey("message") == true ? 
                        errorResponse["message"].ToString() : "Registration failed";

                    _logger.LogWarning("Registration failed for user: {Email}, Error: {Error}", request.Email, errorMessage);
                    return new LoginResponse { Success = false, ErrorMessage = errorMessage };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user: {Email}", request.Email);
                return new LoginResponse
                {
                    Success = false,
                    ErrorMessage = "An error occurred during registration. Please check your connection and try again."
                };
            }
        }

        public async Task<bool> LogoutAsync()
        {
            try
            {
                _logger.LogInformation("Logging out user: {UserId}", CurrentUser?.Id);

                if (!string.IsNullOrEmpty(CurrentState.AccessToken))
                {
                    var response = await _httpClient.PostAsync("/api/auth/logout", null);
                    // Don't fail logout if server request fails
                }

                // Clear current state
                CurrentState = new AuthState();
                _httpClient.DefaultRequestHeaders.Authorization = null;

                AuthenticationStateChanged?.Invoke(this, CurrentState);
                UserLoggedOut?.Invoke(this, EventArgs.Empty);

                _logger.LogInformation("Logout successful");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                // Still clear local state even if server request fails
                CurrentState = new AuthState();
                _httpClient.DefaultRequestHeaders.Authorization = null;
                AuthenticationStateChanged?.Invoke(this, CurrentState);
                UserLoggedOut?.Invoke(this, EventArgs.Empty);
                return true;
            }
        }

        public async Task<bool> RefreshTokenAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(CurrentState.RefreshToken))
                    return false;

                var request = new { RefreshToken = CurrentState.RefreshToken };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/auth/refresh", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var refreshResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (refreshResponse?.Success == true)
                    {
                        CurrentState.AccessToken = refreshResponse.Token;
                        CurrentState.RefreshToken = refreshResponse.RefreshToken;
                        CurrentState.TokenExpiresAt = refreshResponse.ExpiresAt;
                        CurrentState.LastActivity = DateTime.Now;

                        _httpClient.DefaultRequestHeaders.Authorization = 
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", refreshResponse.Token);

                        AuthenticationStateChanged?.Invoke(this, CurrentState);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return false;
            }
        }

        public async Task<bool> ValidateTokenAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(CurrentState.AccessToken))
                    return false;

                var response = await _httpClient.GetAsync("/api/auth/validate");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return false;
            }
        }

        public async Task<UserInfo> GetUserInfoAsync()
        {
            if (CurrentUser != null)
                return CurrentUser;

            try
            {
                var response = await _httpClient.GetAsync("/api/auth/me");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var user = JsonSerializer.Deserialize<UserInfo>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (user != null)
                    {
                        CurrentState.CurrentUser = user;
                        AuthenticationStateChanged?.Invoke(this, CurrentState);
                    }

                    return user ?? new UserInfo();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user info");
            }

            return new UserInfo();
        }

        public async Task<CompanyInfo> GetCompanyInfoAsync()
        {
            if (CurrentCompany != null)
                return CurrentCompany;

            try
            {
                var response = await _httpClient.GetAsync("/api/auth/company");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var company = JsonSerializer.Deserialize<CompanyInfo>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (company != null)
                    {
                        CurrentState.CurrentCompany = company;
                        AuthenticationStateChanged?.Invoke(this, CurrentState);
                    }

                    return company ?? new CompanyInfo();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting company info");
            }

            return new CompanyInfo();
        }

        public async Task<bool> UpdateProfileAsync(ProfileUpdateRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync("/api/auth/profile", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(PasswordChangeRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/auth/change-password", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return false;
            }
        }

        public async Task<bool> UpdateCompanyAsync(CompanyUpdateRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync("/api/auth/company", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating company");
                return false;
            }
        }

        // Enhanced User Profile Management
        public async Task<UserProfileUpdateResponse> UpdateUserProfileAsync(UserProfileUpdateRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync("/api/usermanagement/users/profile", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<UserProfileUpdateResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result?.Success == true && result.User != null)
                    {
                        CurrentState.CurrentUser = result.User;
                        AuthenticationStateChanged?.Invoke(this, CurrentState);
                    }

                    return result ?? new UserProfileUpdateResponse { Success = false, ErrorMessage = "Invalid response" };
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);
                    return new UserProfileUpdateResponse
                    {
                        Success = false,
                        ErrorMessage = errorResponse?.ContainsKey("message") == true ? 
                            errorResponse["message"].ToString() : "Failed to update profile"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile");
                return new UserProfileUpdateResponse
                {
                    Success = false,
                    ErrorMessage = "An error occurred while updating your profile. Please try again."
                };
            }
        }

        public async Task<ChangePasswordResponse> ChangeUserPasswordAsync(ChangePasswordRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/usermanagement/users/change-password", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ChangePasswordResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return result ?? new ChangePasswordResponse { Success = false, ErrorMessage = "Invalid response" };
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);
                    return new ChangePasswordResponse
                    {
                        Success = false,
                        ErrorMessage = errorResponse?.ContainsKey("message") == true ? 
                            errorResponse["message"].ToString() : "Failed to change password"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing user password");
                return new ChangePasswordResponse
                {
                    Success = false,
                    ErrorMessage = "An error occurred while changing your password. Please try again."
                };
            }
        }

        public async Task<UserInfo?> GetCurrentUserAsync()
        {
            return await GetUserInfoAsync();
        }

        public async Task<bool> DeleteAccountAsync(string password)
        {
            try
            {
                var request = new { Password = password };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.DeleteAsync("/api/usermanagement/users/account");
                var success = response.IsSuccessStatusCode;

                if (success)
                {
                    CurrentState = new AuthState();
                    AuthenticationStateChanged?.Invoke(this, CurrentState);
                    UserLoggedOut?.Invoke(this, EventArgs.Empty);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting account");
                return false;
            }
        }

        // Session Management
        public async Task<bool> SaveSessionAsync()
        {
            try
            {
                // In a real implementation, this would save to secure storage
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving session");
                return false;
            }
        }

        public async Task<bool> LoadSessionAsync()
        {
            try
            {
                // In a real implementation, this would load from secure storage
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading session");
                return false;
            }
        }

        public async Task<bool> ClearSessionAsync()
        {
            try
            {
                CurrentState = new AuthState();
                AuthenticationStateChanged?.Invoke(this, CurrentState);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing session");
                return false;
            }
        }

        public async Task<bool> IsSessionValidAsync()
        {
            return await ValidateTokenAsync();
        }

        // Security
        public async Task<bool> ValidateApiKeyAsync(string apiKey)
        {
            try
            {
                var request = new { ApiKey = apiKey };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/auth/validate-api-key", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating API key");
                return false;
            }
        }

        public async Task<string> GenerateApiKeyAsync(string name, DateTime? expiresAt = null)
        {
            try
            {
                var request = new { Name = name, ExpiresAt = expiresAt };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/auth/generate-api-key", content);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseContent);
                    return result?["apiKey"] ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating API key");
            }

            return string.Empty;
        }

        public async Task<List<ApiKeyResponse>> GetApiKeysAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/auth/api-keys");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<ApiKeyResponse>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<ApiKeyResponse>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting API keys");
            }

            return new List<ApiKeyResponse>();
        }

        public async Task<bool> RevokeApiKeyAsync(string keyId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/auth/api-keys/{keyId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking API key");
                return false;
            }
        }

        // Utility Methods
        public async Task<bool> CheckConnectionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/health");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking connection");
                return false;
            }
        }

        public async Task<bool> IsOnlineAsync()
        {
            return await CheckConnectionAsync();
        }

        public void SetOfflineMode(bool isOffline)
        {
            CurrentState.IsOnline = !isOffline;
            AuthenticationStateChanged?.Invoke(this, CurrentState);
        }
    }
}
