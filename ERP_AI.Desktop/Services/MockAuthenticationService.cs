using ERP_AI.Desktop.Models;

namespace ERP_AI.Desktop.Services
{
    public class MockAuthenticationService : IAuthenticationService
    {
        public event EventHandler<AuthState>? AuthenticationStateChanged;
        public event EventHandler<UserInfo>? UserLoggedIn;
        public event EventHandler? UserLoggedOut;

        public AuthState CurrentState { get; private set; } = new();
        public bool IsAuthenticated => CurrentState.IsAuthenticated && !string.IsNullOrEmpty(CurrentState.AccessToken);
        public UserInfo? CurrentUser => CurrentState.CurrentUser;
        public CompanyInfo? CurrentCompany => CurrentState.CurrentCompany;

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            // Simulate network delay
            await Task.Delay(1000);

            // Mock successful login for demo purposes
            if (request.Email == "admin@erpai.com" && request.Password == "password123")
            {
                var user = new UserInfo
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = request.Email,
                    FirstName = "Admin",
                    LastName = "User",
                    CompanyId = Guid.NewGuid().ToString(),
                    CompanyName = "Demo Company",
                    Roles = new List<string> { "Admin" },
                    IsActive = true,
                    LastLogin = DateTime.Now
                };

                var company = new CompanyInfo
                {
                    Id = user.CompanyId,
                    Name = user.CompanyName,
                    Industry = "Technology",
                    Country = "United States",
                    Currency = "USD",
                    TimeZone = "UTC",
                    CreatedAt = DateTime.Now.AddDays(-30),
                    IsActive = true,
                    UserCount = 1,
                    SubscriptionPlan = "Professional"
                };

                var response = new LoginResponse
                {
                    Success = true,
                    Token = "mock-jwt-token-" + Guid.NewGuid().ToString("N")[..8],
                    RefreshToken = "mock-refresh-token-" + Guid.NewGuid().ToString("N")[..8],
                    ExpiresAt = DateTime.UtcNow.AddHours(8),
                    User = user
                };

                CurrentState = new AuthState
                {
                    IsAuthenticated = true,
                    CurrentUser = user,
                    CurrentCompany = company,
                    AccessToken = response.Token,
                    RefreshToken = response.RefreshToken,
                    TokenExpiresAt = response.ExpiresAt,
                    IsOnline = true,
                    LastActivity = DateTime.Now
                };

                AuthenticationStateChanged?.Invoke(this, CurrentState);
                UserLoggedIn?.Invoke(this, user);

                return response;
            }

            return new LoginResponse
            {
                Success = false,
                ErrorMessage = "Invalid email or password. Try admin@erpai.com / password123"
            };
        }

        public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
        {
            // Simulate network delay
            await Task.Delay(1500);

            // Mock successful registration
            var user = new UserInfo
            {
                Id = Guid.NewGuid().ToString(),
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                CompanyId = Guid.NewGuid().ToString(),
                CompanyName = request.CompanyName,
                Roles = new List<string> { "User" },
                IsActive = true,
                LastLogin = DateTime.Now
            };

            var company = new CompanyInfo
            {
                Id = user.CompanyId,
                Name = request.CompanyName,
                Industry = "Technology",
                Country = "United States",
                Currency = "USD",
                TimeZone = "UTC",
                CreatedAt = DateTime.Now,
                IsActive = true,
                UserCount = 1,
                SubscriptionPlan = "Free"
            };

            var response = new LoginResponse
            {
                Success = true,
                Token = "mock-jwt-token-" + Guid.NewGuid().ToString("N")[..8],
                RefreshToken = "mock-refresh-token-" + Guid.NewGuid().ToString("N")[..8],
                ExpiresAt = DateTime.UtcNow.AddHours(8),
                User = user
            };

            CurrentState = new AuthState
            {
                IsAuthenticated = true,
                CurrentUser = user,
                CurrentCompany = company,
                AccessToken = response.Token,
                RefreshToken = response.RefreshToken,
                TokenExpiresAt = response.ExpiresAt,
                IsOnline = true,
                LastActivity = DateTime.Now
            };

            AuthenticationStateChanged?.Invoke(this, CurrentState);
            UserLoggedIn?.Invoke(this, user);

            return response;
        }

        public async Task<bool> LogoutAsync()
        {
            await Task.Delay(500);
            CurrentState = new AuthState();
            AuthenticationStateChanged?.Invoke(this, CurrentState);
            UserLoggedOut?.Invoke(this, EventArgs.Empty);
            return true;
        }

        public async Task<bool> RefreshTokenAsync()
        {
            await Task.Delay(500);
            return true;
        }

        public async Task<bool> ValidateTokenAsync()
        {
            await Task.Delay(100);
            return IsAuthenticated && CurrentState.TokenExpiresAt > DateTime.UtcNow;
        }

        public async Task<UserInfo> GetUserInfoAsync()
        {
            await Task.Delay(200);
            return CurrentUser ?? throw new UnauthorizedAccessException("User not authenticated");
        }

        public async Task<CompanyInfo> GetCompanyInfoAsync()
        {
            await Task.Delay(200);
            return CurrentCompany ?? throw new UnauthorizedAccessException("User not authenticated");
        }

        public async Task<bool> UpdateProfileAsync(ProfileUpdateRequest request)
        {
            await Task.Delay(500);
            return true;
        }

        public async Task<bool> ChangePasswordAsync(PasswordChangeRequest request)
        {
            await Task.Delay(500);
            return true;
        }

        public async Task<bool> UpdateCompanyAsync(CompanyUpdateRequest request)
        {
            await Task.Delay(500);
            return true;
        }

        public async Task<bool> SaveSessionAsync()
        {
            await Task.Delay(100);
            return true;
        }

        public async Task<bool> LoadSessionAsync()
        {
            await Task.Delay(100);
            return false; // No saved session for demo
        }

        public async Task<bool> ClearSessionAsync()
        {
            await Task.Delay(100);
            CurrentState = new AuthState();
            AuthenticationStateChanged?.Invoke(this, CurrentState);
            return true;
        }

        public async Task<bool> IsSessionValidAsync()
        {
            await Task.Delay(50);
            return IsAuthenticated && CurrentState.TokenExpiresAt > DateTime.UtcNow.AddMinutes(5);
        }

        public async Task<bool> ValidateApiKeyAsync(string apiKey)
        {
            await Task.Delay(100);
            return !string.IsNullOrEmpty(apiKey);
        }

        public async Task<string> GenerateApiKeyAsync(string name, DateTime? expiresAt = null)
        {
            await Task.Delay(200);
            return "mock-api-key-" + Guid.NewGuid().ToString("N")[..16];
        }

        public async Task<List<ApiKeyResponse>> GetApiKeysAsync()
        {
            await Task.Delay(200);
            return new List<ApiKeyResponse>();
        }

        public async Task<bool> RevokeApiKeyAsync(string keyId)
        {
            await Task.Delay(200);
            return true;
        }

        public async Task<bool> CheckConnectionAsync()
        {
            await Task.Delay(100);
            return true; // Always online for demo
        }

        public async Task<bool> IsOnlineAsync()
        {
            await Task.Delay(50);
            return true; // Always online for demo
        }

        public void SetOfflineMode(bool isOffline)
        {
            CurrentState.IsOnline = !isOffline;
            AuthenticationStateChanged?.Invoke(this, CurrentState);
        }
    }
}

