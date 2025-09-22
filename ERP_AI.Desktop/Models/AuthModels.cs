using System.ComponentModel.DataAnnotations;

namespace ERP_AI.Desktop.Models
{
    // Desktop Authentication Models
    
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;
        
        public bool RememberMe { get; set; } = false;
        public string? CompanyId { get; set; }
    }

    public class RegisterRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "First Name is required")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Last Name is required")]
        public string LastName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Company Name is required")]
        public string CompanyName { get; set; } = string.Empty;
        
        public string? PhoneNumber { get; set; }
        
        public bool AcceptTerms { get; set; } = false;
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UserInfo User { get; set; } = new();
        public CompanyInfo? Company { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class UserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string? PhoneNumber { get; set; }
        public string? Company { get; set; }
        public string CompanyId { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public bool IsActive { get; set; } = true;
        public DateTime LastLogin { get; set; }
        public string? ProfileImage { get; set; }
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

    public class AuthState
    {
        public bool IsAuthenticated { get; set; }
        public UserInfo? CurrentUser { get; set; }
        public CompanyInfo? CurrentCompany { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? TokenExpiresAt { get; set; }
        public bool IsOnline { get; set; } = true;
        public DateTime LastActivity { get; set; } = DateTime.Now;
    }

    public class PasswordChangeRequest
    {
        [Required(ErrorMessage = "Current password is required")]
        public string CurrentPassword { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "New password is required")]
        [MinLength(6, ErrorMessage = "New password must be at least 6 characters")]
        public string NewPassword { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Confirm new password is required")]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    public class ProfileUpdateRequest
    {
        [Required(ErrorMessage = "First Name is required")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Last Name is required")]
        public string LastName { get; set; } = string.Empty;
        
        public string? PhoneNumber { get; set; }
        public string? ProfileImage { get; set; }
    }

    public class CompanyUpdateRequest
    {
        [Required(ErrorMessage = "Company Name is required")]
        public string Name { get; set; } = string.Empty;
        
        public string Industry { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Currency { get; set; } = "USD";
        public string TimeZone { get; set; } = "UTC";
    }

    public class UserProfileUpdateRequest
    {
        [Required(ErrorMessage = "First Name is required")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Last Name is required")]
        public string LastName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;
        
        public string? PhoneNumber { get; set; }
        public string? Company { get; set; }
        public string Theme { get; set; } = "Light";
        public string Language { get; set; } = "English";
        public bool EmailNotifications { get; set; } = true;
        public bool PushNotifications { get; set; } = true;
        public bool SecurityAlerts { get; set; } = true;
    }

    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Current password is required")]
        public string CurrentPassword { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "New password is required")]
        [MinLength(8, ErrorMessage = "New password must be at least 8 characters")]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class BaseResponse
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class UserProfileUpdateResponse : BaseResponse
    {
        public UserInfo? User { get; set; }
    }

    public class ChangePasswordResponse : BaseResponse
    {
        public bool PasswordChanged { get; set; }
    }
}

