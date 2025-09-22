using Microsoft.AspNetCore.Mvc;
using ERP_AI.CloudAPI.Models;
using ERP_AI.CloudAPI.Services;
using Microsoft.AspNetCore.Authorization;

namespace ERP_AI.CloudAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserManagementController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<UserManagementController> _logger;

        public UserManagementController(IAuthService authService, ILogger<UserManagementController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpGet("users")]
        public async Task<ActionResult<List<UserInfo>>> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var users = await _authService.GetUsersAsync(page, pageSize);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("users/{id}")]
        public async Task<ActionResult<UserInfo>> GetUser(string id)
        {
            try
            {
                var user = await _authService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound();

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {UserId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("users")]
        public async Task<ActionResult<UserInfo>> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                var user = await _authService.CreateUserAsync(request);
                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("users/{id}")]
        public async Task<ActionResult<UserInfo>> UpdateUser(string id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                var user = await _authService.UpdateUserAsync(id, request);
                if (user == null)
                    return NotFound();

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("users/{id}")]
        public async Task<ActionResult> DeleteUser(string id)
        {
            try
            {
                var success = await _authService.DeleteUserAsync(id);
                if (!success)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("users/{id}/reset-password")]
        public async Task<ActionResult> ResetPassword(string id, [FromBody] ResetPasswordRequest request)
        {
            try
            {
                var success = await _authService.ResetPasswordAsync(id, request);
                if (!success)
                    return NotFound();

                return Ok(new { message = "Password reset email sent" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for user {UserId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("audit-logs")]
        public async Task<ActionResult<List<AuditLog>>> GetAuditLogs([FromQuery] string? userId = null, [FromQuery] string? eventType = null, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var logs = await _authService.GetAuditLogsAsync(userId, eventType, fromDate, toDate);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audit logs");
                return StatusCode(500, "Internal server error");
            }
        }
    }

    public class CreateUserRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string CompanyId { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }

    public class UpdateUserRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public List<string> Roles { get; set; } = new();
        public bool IsActive { get; set; } = true;
    }

    public class ResetPasswordRequest
    {
        public string NewPassword { get; set; } = string.Empty;
    }
}
