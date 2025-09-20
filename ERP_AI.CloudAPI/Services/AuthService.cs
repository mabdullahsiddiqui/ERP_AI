using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ERP_AI.CloudAPI.Models;
using ERP_AI.Data;

namespace ERP_AI.CloudAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ERPDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ERPDbContext context,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        ErrorMessage = "Invalid email or password"
                    };
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
                if (!result.Succeeded)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        ErrorMessage = "Invalid email or password"
                    };
                }

                // Get user info
                var userInfo = await GetUserInfoAsync(user.Id);

                // Validate company access if specified
                if (!string.IsNullOrEmpty(request.CompanyId) && userInfo.CompanyId != request.CompanyId)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        ErrorMessage = "Access denied to specified company"
                    };
                }

                // Generate tokens
                var token = await GenerateJwtTokenAsync(userInfo);
                var refreshToken = await GenerateRefreshTokenAsync(user.Id);

                return new LoginResponse
                {
                    Success = true,
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddHours(24),
                    User = userInfo
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
                return new LoginResponse
                {
                    Success = false,
                    ErrorMessage = "An error occurred during login"
                };
            }
        }

        public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        ErrorMessage = "User with this email already exists"
                    };
                }

                // Create new user
                var user = new IdentityUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    EmailConfirmed = true // For demo purposes
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        ErrorMessage = string.Join(", ", result.Errors.Select(e => e.Description))
                    };
                }

                // Create company
                var company = new Company
                {
                    Name = request.CompanyName,
                    Industry = "General",
                    Country = "US",
                    Currency = "USD",
                    TimeZone = "UTC",
                    IsActive = true
                };

                _context.Companies.Add(company);
                await _context.SaveChangesAsync();

                // Create user profile
                var userProfile = new User
                {
                    Id = Guid.Parse(user.Id),
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    CompanyId = company.Id,
                    IsActive = true,
                    Role = "Admin"
                };

                _context.Users.Add(userProfile);
                await _context.SaveChangesAsync();

                // Generate tokens
                var userInfo = await GetUserInfoAsync(user.Id);
                var token = await GenerateJwtTokenAsync(userInfo);
                var refreshToken = await GenerateRefreshTokenAsync(user.Id);

                return new LoginResponse
                {
                    Success = true,
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddHours(24),
                    User = userInfo
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for email: {Email}", request.Email);
                return new LoginResponse
                {
                    Success = false,
                    ErrorMessage = "An error occurred during registration"
                };
            }
        }

        public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            try
            {
                // Validate refresh token (simplified implementation)
                var principal = GetPrincipalFromExpiredToken(request.Token);
                if (principal == null)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        ErrorMessage = "Invalid token"
                    };
                }

                var userId = principal.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return new LoginResponse
                    {
                        Success = false,
                        ErrorMessage = "Invalid token"
                    };
                }

                var userInfo = await GetUserInfoAsync(userId);
                var newToken = await GenerateJwtTokenAsync(userInfo);
                var newRefreshToken = await GenerateRefreshTokenAsync(userId);

                return new LoginResponse
                {
                    Success = true,
                    Token = newToken,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddHours(24),
                    User = userInfo
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return new LoginResponse
                {
                    Success = false,
                    ErrorMessage = "An error occurred during token refresh"
                };
            }
        }

        public async Task LogoutAsync(string userId)
        {
            // Invalidate refresh tokens (would be stored in database in production)
            _logger.LogInformation("User logged out: {UserId}", userId);
            await Task.CompletedTask;
        }

        public async Task<ApiKeyResponse> CreateApiKeyAsync(ApiKeyRequest request, string userId, string companyId)
        {
            var apiKey = GenerateApiKey();
            
            // In production, store API key in database with proper hashing
            var response = new ApiKeyResponse
            {
                Id = Guid.NewGuid().ToString(),
                Name = request.Name,
                Key = apiKey,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = request.ExpiresAt,
                Permissions = request.Permissions,
                IsActive = true
            };

            _logger.LogInformation("API key created for user {UserId}", userId);
            return response;
        }

        public async Task<List<ApiKeyResponse>> GetApiKeysAsync(string userId)
        {
            // In production, retrieve from database
            return new List<ApiKeyResponse>();
        }

        public async Task RevokeApiKeyAsync(string keyId, string userId)
        {
            // In production, mark API key as revoked in database
            _logger.LogInformation("API key {KeyId} revoked by user {UserId}", keyId, userId);
            await Task.CompletedTask;
        }

        public async Task<UserInfo> GetUserInfoAsync(string userId)
        {
            var user = await _context.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id.ToString() == userId);

            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            return new UserInfo
            {
                Id = user.Id.ToString(),
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CompanyId = user.CompanyId.ToString(),
                CompanyName = user.Company?.Name ?? "Unknown",
                Roles = new List<string> { user.Role },
                IsActive = user.IsActive,
                LastLogin = DateTime.UtcNow
            };
        }

        public async Task<CompanyInfo> GetCompanyInfoAsync(string companyId)
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Id.ToString() == companyId);

            if (company == null)
            {
                throw new InvalidOperationException("Company not found");
            }

            var userCount = await _context.Users.CountAsync(u => u.CompanyId.ToString() == companyId);

            return new CompanyInfo
            {
                Id = company.Id.ToString(),
                Name = company.Name,
                Industry = company.Industry,
                Country = company.Country,
                Currency = company.Currency,
                TimeZone = company.TimeZone,
                CreatedAt = company.CreatedAt,
                IsActive = company.IsActive,
                UserCount = userCount,
                SubscriptionPlan = "Pro" // Default for demo
            };
        }

        public async Task<bool> ValidateApiKeyAsync(string apiKey, string companyId)
        {
            // In production, validate against database
            return !string.IsNullOrEmpty(apiKey) && apiKey.Length > 20;
        }

        public async Task<string> GenerateJwtTokenAsync(UserInfo user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "your-super-secret-key-that-is-at-least-32-characters-long"));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("sub", user.Id),
                new Claim("email", user.Email),
                new Claim("given_name", user.FirstName),
                new Claim("family_name", user.LastName),
                new Claim("CompanyId", user.CompanyId),
                new Claim("role", string.Join(",", user.Roles))
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "ERP_AI_CloudAPI",
                audience: _configuration["Jwt:Audience"] ?? "ERP_AI_Desktop",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> GenerateRefreshTokenAsync(string userId)
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public async Task<UserInfo?> ValidateJwtTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "your-super-secret-key-that-is-at-least-32-characters-long");

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"] ?? "ERP_AI_CloudAPI",
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"] ?? "ERP_AI_Desktop",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                var userId = principal.FindFirst("sub")?.Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    return await GetUserInfoAsync(userId);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating JWT token");
                return null;
            }
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "your-super-secret-key-that-is-at-least-32-characters-long");

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false // Don't validate lifetime for refresh
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                return principal;
            }
            catch
            {
                return null;
            }
        }

        private string GenerateApiKey()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return $"erpai_{Convert.ToBase64String(randomBytes).Replace("+", "").Replace("/", "").Replace("=", "")}";
        }
    }
}
