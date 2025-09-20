using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace ERP_AI.CloudSync
{
    public class CloudApiClient
    {
        private readonly RestClient _client;
        private readonly ILogger<CloudApiClient> _logger;
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public CloudApiClient(ILogger<CloudApiClient> logger, string baseUrl = "https://api.erp-ai.com", string apiKey = "")
        {
            _logger = logger;
            _baseUrl = baseUrl;
            _apiKey = apiKey;
            _client = new RestClient(baseUrl);
            
            // Add default headers
            _client.AddDefaultHeader("Authorization", $"Bearer {apiKey}");
            _client.AddDefaultHeader("Content-Type", "application/json");
            _client.AddDefaultHeader("User-Agent", "ERP_AI_Desktop/1.0");
        }

        public async Task<ApiResponse<T>> GetAsync<T>(string endpoint, Dictionary<string, object>? parameters = null)
        {
            try
            {
                var request = new RestRequest(endpoint, Method.Get);
                
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        request.AddParameter(param.Key, param.Value);
                    }
                }

                var response = await _client.ExecuteAsync<T>(request);
                return new ApiResponse<T>
                {
                    IsSuccess = response.IsSuccessful,
                    Data = response.Data,
                    ErrorMessage = response.ErrorMessage,
                    StatusCode = response.StatusCode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GET request to {endpoint}");
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                var request = new RestRequest(endpoint, Method.Post);
                request.AddJsonBody(data);

                var response = await _client.ExecuteAsync<T>(request);
                return new ApiResponse<T>
                {
                    IsSuccess = response.IsSuccessful,
                    Data = response.Data,
                    ErrorMessage = response.ErrorMessage,
                    StatusCode = response.StatusCode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in POST request to {endpoint}");
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data)
        {
            try
            {
                var request = new RestRequest(endpoint, Method.Put);
                request.AddJsonBody(data);

                var response = await _client.ExecuteAsync<T>(request);
                return new ApiResponse<T>
                {
                    IsSuccess = response.IsSuccessful,
                    Data = response.Data,
                    ErrorMessage = response.ErrorMessage,
                    StatusCode = response.StatusCode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in PUT request to {endpoint}");
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<T>> DeleteAsync<T>(string endpoint)
        {
            try
            {
                var request = new RestRequest(endpoint, Method.Delete);

                var response = await _client.ExecuteAsync<T>(request);
                return new ApiResponse<T>
                {
                    IsSuccess = response.IsSuccessful,
                    Data = response.Data,
                    ErrorMessage = response.ErrorMessage,
                    StatusCode = response.StatusCode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in DELETE request to {endpoint}");
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var response = await GetAsync<object>("/health");
                return response.IsSuccess;
            }
            catch
            {
                return false;
            }
        }

        public async Task<ApiResponse<SyncChangesResponse>> GetSyncChangesAsync(string lastSyncTime)
        {
            var parameters = new Dictionary<string, object>
            {
                { "lastSync", lastSyncTime }
            };

            return await GetAsync<SyncChangesResponse>("/sync/changes", parameters);
        }

        public async Task<ApiResponse<SyncResponse>> PushEntityAsync(string entityType, object entity)
        {
            return await PostAsync<SyncResponse>($"/sync/{entityType.ToLower()}", entity);
        }

        public async Task<ApiResponse<SyncResponse>> UpdateEntityAsync(string entityType, string entityId, object entity)
        {
            return await PutAsync<SyncResponse>($"/sync/{entityType.ToLower()}/{entityId}", entity);
        }

        public async Task<ApiResponse<SyncResponse>> DeleteEntityAsync(string entityType, string entityId)
        {
            return await DeleteAsync<SyncResponse>($"/sync/{entityType.ToLower()}/{entityId}");
        }

        public async Task<ApiResponse<AuthResponse>> AuthenticateAsync(string username, string password)
        {
            var authData = new
            {
                username,
                password,
                deviceId = Environment.MachineName,
                appVersion = "1.0.0"
            };

            return await PostAsync<AuthResponse>("/auth/login", authData);
        }

        public async Task<ApiResponse<object>> RefreshTokenAsync(string refreshToken)
        {
            var tokenData = new { refreshToken };
            return await PostAsync<object>("/auth/refresh", tokenData);
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }

    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public int StatusCode { get; set; }
    }

    public class SyncChangesResponse
    {
        public List<SyncChange> Changes { get; set; } = new();
        public string LastSyncTime { get; set; } = string.Empty;
        public bool HasMore { get; set; }
    }

    public class SyncResponse
    {
        public bool Success { get; set; }
        public string? CloudId { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class AuthResponse
    {
        public bool Success { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public UserInfo? User { get; set; }
    }

    public class UserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CompanyId { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new();
    }
}
