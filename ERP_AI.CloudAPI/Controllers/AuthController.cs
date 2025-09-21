using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ERP_AI.CloudAPI.Models;
using ERP_AI.CloudAPI.Services;

namespace ERP_AI.CloudAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Authenticate user and return JWT token
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Login attempt for email: {Email}", request.Email);

                var response = await _authService.LoginAsync(request);

                if (response.Success)
                {
                    _logger.LogInformation("Successful login for user: {Email}", request.Email);
                    return Ok(ApiResponse.SuccessResult(response));
                }

                _logger.LogWarning("Failed login attempt for email: {Email}", request.Email);
                return Unauthorized(ApiResponse.ErrorResult<LoginResponse>(
                    response.ErrorMessage ?? "Invalid credentials", "AUTH_FAILED"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
                return StatusCode(500, ApiResponse.ErrorResult<LoginResponse>(
                    "Internal server error during authentication", "AUTH_ERROR"));
            }
        }

        /// <summary>
        /// Register new user and company
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                _logger.LogInformation("Registration attempt for email: {Email}", request.Email);

                var response = await _authService.RegisterAsync(request);

                if (response.Success)
                {
                    _logger.LogInformation("Successful registration for user: {Email}", request.Email);
                    return Ok(ApiResponse.SuccessResult(response));
                }

                _logger.LogWarning("Failed registration attempt for email: {Email}", request.Email);
                return BadRequest(ApiResponse.ErrorResult<LoginResponse>(
                    response.ErrorMessage ?? "Registration failed", "REGISTRATION_FAILED"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for email: {Email}", request.Email);
                return StatusCode(500, ApiResponse.ErrorResult<LoginResponse>(
                    "Internal server error during registration", "REGISTRATION_ERROR"));
            }
        }

        /// <summary>
        /// Refresh JWT token using refresh token
        /// </summary>
        [HttpPost("refresh")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var response = await _authService.RefreshTokenAsync(request);

                if (response.Success)
                {
                    return Ok(ApiResponse.SuccessResult(response));
                }

                return Unauthorized(ApiResponse.ErrorResult<LoginResponse>(
                    response.ErrorMessage ?? "Invalid refresh token", "REFRESH_FAILED"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return StatusCode(500, ApiResponse.ErrorResult<LoginResponse>(
                    "Internal server error during token refresh", "REFRESH_ERROR"));
            }
        }

        /// <summary>
        /// Logout user and invalidate tokens
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> Logout()
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value ?? string.Empty;
                await _authService.LogoutAsync(userId);

                _logger.LogInformation("User logged out: {UserId}", userId);
                return Ok(ApiResponse.SuccessResult(new { Message = "Logged out successfully" }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, ApiResponse.ErrorResult<object>(
                    "Internal server error during logout", "LOGOUT_ERROR"));
            }
        }

        /// <summary>
        /// Generate API key for desktop application
        /// </summary>
        [HttpPost("api-keys")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<ApiKeyResponse>>> CreateApiKey([FromBody] ApiKeyRequest request)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value ?? string.Empty;
                var companyId = User.FindFirst("CompanyId")?.Value ?? string.Empty;

                var response = await _authService.CreateApiKeyAsync(request, userId, companyId);

                _logger.LogInformation("API key created for user: {UserId}", userId);
                return Ok(ApiResponse.SuccessResult(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating API key");
                return StatusCode(500, ApiResponse.ErrorResult<ApiKeyResponse>(
                    "Internal server error creating API key", "API_KEY_ERROR"));
            }
        }

        /// <summary>
        /// Get list of API keys for user
        /// </summary>
        [HttpGet("api-keys")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<List<ApiKeyResponse>>>> GetApiKeys()
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value ?? string.Empty;
                var apiKeys = await _authService.GetApiKeysAsync(userId);

                return Ok(ApiResponse.SuccessResult(apiKeys));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting API keys");
                return StatusCode(500, ApiResponse.ErrorResult<List<ApiKeyResponse>>(
                    "Internal server error getting API keys", "GET_API_KEYS_ERROR"));
            }
        }

        /// <summary>
        /// Revoke API key
        /// </summary>
        [HttpDelete("api-keys/{keyId}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> RevokeApiKey(string keyId)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value ?? string.Empty;
                await _authService.RevokeApiKeyAsync(keyId, userId);

                _logger.LogInformation("API key revoked: {KeyId} by user: {UserId}", keyId, userId);
                return Ok(ApiResponse.SuccessResult(new { Message = "API key revoked successfully" }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking API key: {KeyId}", keyId);
                return StatusCode(500, ApiResponse.ErrorResult<object>(
                    "Internal server error revoking API key", "REVOKE_API_KEY_ERROR"));
            }
        }

        /// <summary>
        /// Get current user information
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserInfo>>> GetCurrentUser()
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value ?? string.Empty;
                var userInfo = await _authService.GetUserInfoAsync(userId);

                return Ok(ApiResponse.SuccessResult(userInfo));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user info");
                return StatusCode(500, ApiResponse.ErrorResult<UserInfo>(
                    "Internal server error getting user info", "GET_USER_INFO_ERROR"));
            }
        }

        /// <summary>
        /// Get company information
        /// </summary>
        [HttpGet("company")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<CompanyInfo>>> GetCompanyInfo()
        {
            try
            {
                var companyId = User.FindFirst("CompanyId")?.Value ?? string.Empty;
                var companyInfo = await _authService.GetCompanyInfoAsync(companyId);

                return Ok(ApiResponse.SuccessResult(companyInfo));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting company info");
                return StatusCode(500, ApiResponse.ErrorResult<CompanyInfo>(
                    "Internal server error getting company info", "GET_COMPANY_INFO_ERROR"));
            }
        }
    }
}
