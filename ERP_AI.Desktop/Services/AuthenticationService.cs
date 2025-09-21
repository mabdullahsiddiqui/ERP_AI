using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using ERP_AI.Desktop.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace ERP_AI.Desktop.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _baseUrl;
        private readonly string _sessionFilePath;

        public event EventHandler<AuthState>? AuthenticationStateChanged;
        public event EventHandler<UserInfo>? UserLoggedIn;
        public event EventHandler? UserLoggedOut;

        public AuthState CurrentState { get; private set; } = new();
        public bool IsAuthenticated => CurrentState.IsAuthenticated && !string.IsNullOrEmpty(CurrentState.AccessToken);
        public UserInfo? CurrentUser => CurrentState.CurrentUser;
        public CompanyInfo? CurrentCompany => CurrentState.CurrentCompany;

        public AuthenticationService(HttpClient httpClient, ILogger<AuthenticationService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
            _baseUrl = _configuration["CloudAPI:BaseUrl"] ?? "https://localhost:7001";
            _sessionFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ERP_AI", "session.json");

            // Set up HTTP client
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "ERP_AI_Desktop/1.0");

            // Load existing session on startup
            _ = Task.Run(LoadSessionAsync);
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Attempting login for user: {Email}", request.Email);

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/v1/auth/login", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<LoginResponse>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (apiResponse?.Data != null)
                    {
                        await UpdateAuthState(apiResponse.Data, request.RememberMe);
                        UserLoggedIn?.Invoke(this, apiResponse.Data.User);
                        _logger.LogInformation("Login successful for user: {Email}", request.Email);
                        return apiResponse.Data;
                    }
                }

                var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return new LoginResponse
                {
                    Success = false,
                    ErrorMessage = errorResponse?.Message ?? "Login failed"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {Email}", request.Email);
                return new LoginResponse
                {
                    Success = false,
                    ErrorMessage = "Network error. Please check your connection."
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

                var response = await _httpClient.PostAsync("/api/v1/auth/register", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<LoginResponse>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (apiResponse?.Data != null)
                    {
                        await UpdateAuthState(apiResponse.Data, true);
                        UserLoggedIn?.Invoke(this, apiResponse.Data.User);
                        _logger.LogInformation("Registration successful for user: {Email}", request.Email);
                        return apiResponse.Data;
                    }
                }

                var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return new LoginResponse
                {
                    Success = false,
                    ErrorMessage = errorResponse?.Message ?? "Registration failed"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user: {Email}", request.Email);
                return new LoginResponse
                {
                    Success = false,
                    ErrorMessage = "Network error. Please check your connection."
                };
            }
        }

        public async Task<bool> LogoutAsync()
        {
            try
            {
                if (IsAuthenticated)
                {
                    _httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CurrentState.AccessToken);

                    await _httpClient.PostAsync("/api/v1/auth/logout", null);
                }

                await ClearSessionAsync();
                UserLoggedOut?.Invoke(this, EventArgs.Empty);
                _logger.LogInformation("User logged out successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return false;
            }
        }

        public async Task<bool> RefreshTokenAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(CurrentState.RefreshToken))
                    return false;

                var request = new { Token = CurrentState.AccessToken, RefreshToken = CurrentState.RefreshToken };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/v1/auth/refresh", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<LoginResponse>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (apiResponse?.Data != null)
                    {
                        await UpdateAuthState(apiResponse.Data, true);
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
                if (!IsAuthenticated || CurrentState.TokenExpiresAt <= DateTime.UtcNow)
                    return false;

                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CurrentState.AccessToken);

                var response = await _httpClient.GetAsync("/api/v1/auth/me");
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
            try
            {
                if (!IsAuthenticated)
                    throw new UnauthorizedAccessException("User not authenticated");

                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CurrentState.AccessToken);

                var response = await _httpClient.GetAsync("/api/v1/auth/me");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<UserInfo>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (apiResponse?.Data != null)
                    {
                        CurrentState.CurrentUser = apiResponse.Data;
                        AuthenticationStateChanged?.Invoke(this, CurrentState);
                        return apiResponse.Data;
                    }
                }

                throw new Exception("Failed to get user info");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user info");
                throw;
            }
        }

        public async Task<CompanyInfo> GetCompanyInfoAsync()
        {
            try
            {
                if (!IsAuthenticated)
                    throw new UnauthorizedAccessException("User not authenticated");

                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CurrentState.AccessToken);

                var response = await _httpClient.GetAsync("/api/v1/auth/company");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<CompanyInfo>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (apiResponse?.Data != null)
                    {
                        CurrentState.CurrentCompany = apiResponse.Data;
                        AuthenticationStateChanged?.Invoke(this, CurrentState);
                        return apiResponse.Data;
                    }
                }

                throw new Exception("Failed to get company info");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting company info");
                throw;
            }
        }

        public async Task<bool> UpdateProfileAsync(ProfileUpdateRequest request)
        {
            try
            {
                if (!IsAuthenticated)
                    return false;

                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CurrentState.AccessToken);

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync("/api/v1/auth/profile", content);
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
                if (!IsAuthenticated)
                    return false;

                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CurrentState.AccessToken);

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync("/api/v1/auth/change-password", content);
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
                if (!IsAuthenticated)
                    return false;

                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CurrentState.AccessToken);

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync("/api/v1/auth/company", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating company");
                return false;
            }
        }

        public async Task<bool> SaveSessionAsync()
        {
            try
            {
                var directory = Path.GetDirectoryName(_sessionFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var sessionData = JsonSerializer.Serialize(CurrentState, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                await File.WriteAllTextAsync(_sessionFilePath, sessionData);
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
                if (!File.Exists(_sessionFilePath))
                    return false;

                var sessionData = await File.ReadAllTextAsync(_sessionFilePath);
                var state = JsonSerializer.Deserialize<AuthState>(sessionData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (state != null && !string.IsNullOrEmpty(state.AccessToken))
                {
                    CurrentState = state;
                    CurrentState.LastActivity = DateTime.Now;

                    // Validate token
                    if (await ValidateTokenAsync())
                    {
                        AuthenticationStateChanged?.Invoke(this, CurrentState);
                        return true;
                    }
                    else
                    {
                        await ClearSessionAsync();
                    }
                }

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

                if (File.Exists(_sessionFilePath))
                {
                    File.Delete(_sessionFilePath);
                }

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
            return IsAuthenticated && CurrentState.TokenExpiresAt > DateTime.UtcNow.AddMinutes(5);
        }

        public async Task<bool> ValidateApiKeyAsync(string apiKey)
        {
            try
            {
                if (!IsAuthenticated)
                    return false;

                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CurrentState.AccessToken);

                var response = await _httpClient.PostAsync($"/api/v1/auth/validate-api-key?apiKey={apiKey}", null);
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
                if (!IsAuthenticated)
                    throw new UnauthorizedAccessException("User not authenticated");

                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CurrentState.AccessToken);

                var request = new { Name = name, ExpiresAt = expiresAt };
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/v1/auth/api-keys", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<ApiKeyResponse>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return apiResponse?.Data?.Key ?? string.Empty;
                }

                throw new Exception("Failed to generate API key");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating API key");
                throw;
            }
        }

        public async Task<List<ApiKeyResponse>> GetApiKeysAsync()
        {
            try
            {
                if (!IsAuthenticated)
                    throw new UnauthorizedAccessException("User not authenticated");

                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CurrentState.AccessToken);

                var response = await _httpClient.GetAsync("/api/v1/auth/api-keys");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<ApiKeyResponse>>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return apiResponse?.Data ?? new List<ApiKeyResponse>();
                }

                return new List<ApiKeyResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting API keys");
                return new List<ApiKeyResponse>();
            }
        }

        public async Task<bool> RevokeApiKeyAsync(string keyId)
        {
            try
            {
                if (!IsAuthenticated)
                    return false;

                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CurrentState.AccessToken);

                var response = await _httpClient.DeleteAsync($"/api/v1/auth/api-keys/{keyId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking API key");
                return false;
            }
        }

        public async Task<bool> CheckConnectionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/v1/health");
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

        private async Task UpdateAuthState(LoginResponse response, bool rememberMe)
        {
            CurrentState = new AuthState
            {
                IsAuthenticated = response.Success,
                CurrentUser = response.User,
                AccessToken = response.Token,
                RefreshToken = response.RefreshToken,
                TokenExpiresAt = response.ExpiresAt,
                IsOnline = true,
                LastActivity = DateTime.Now
            };

            if (rememberMe)
            {
                await SaveSessionAsync();
            }

            AuthenticationStateChanged?.Invoke(this, CurrentState);
        }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public string? ErrorCode { get; set; }
    }
}
