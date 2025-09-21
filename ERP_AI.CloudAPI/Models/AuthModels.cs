using System.ComponentModel.DataAnnotations;

namespace ERP_AI.CloudAPI.Models
{
    // Authentication and User Management Models
    
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
        
        public string? CompanyId { get; set; }
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UserInfo User { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }

    public class RefreshTokenRequest
    {
        [Required]
        public string Token { get; set; } = string.Empty;
        
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
        
        [Required]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        public string CompanyName { get; set; } = string.Empty;
        
        public string? PhoneNumber { get; set; }
    }

    public class UserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string CompanyId { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public bool IsActive { get; set; } = true;
        public DateTime LastLogin { get; set; }
    }

    public class ApiKeyRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public DateTime? ExpiresAt { get; set; }
        
        public List<string> Permissions { get; set; } = new();
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

    public class CompanyInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Currency { get; set; } = "USD";
        public string TimeZone { get; set; } = "UTC";
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public int UserCount { get; set; }
        public string SubscriptionPlan { get; set; } = "Free";
    }
}
