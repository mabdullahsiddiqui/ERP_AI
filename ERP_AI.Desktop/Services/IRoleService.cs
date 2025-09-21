using ERP_AI.Desktop.Models;

namespace ERP_AI.Desktop.Services
{
    public interface IRoleService
    {
        // Role Management
        Task<List<Role>> GetRolesAsync();
        Task<Role?> GetRoleByIdAsync(string roleId);
        Task<Role> CreateRoleAsync(CreateRoleRequest request);
        Task<bool> UpdateRoleAsync(string roleId, UpdateRoleRequest request);
        Task<bool> DeleteRoleAsync(string roleId);
        
        // User Role Assignment
        Task<bool> AssignRoleToUserAsync(string userId, string roleId);
        Task<bool> RemoveRoleFromUserAsync(string userId, string roleId);
        Task<List<Role>> GetUserRolesAsync(string userId);
        Task<bool> HasRoleAsync(string userId, string roleName);
        Task<bool> HasPermissionAsync(string userId, string permission);
        
        // Permission Management
        Task<List<Permission>> GetPermissionsAsync();
        Task<List<Permission>> GetRolePermissionsAsync(string roleId);
        Task<bool> AddPermissionToRoleAsync(string roleId, string permission);
        Task<bool> RemovePermissionFromRoleAsync(string roleId, string permission);
        
        // Role-based UI Control
        bool CanAccessFeature(string featureName);
        bool CanPerformAction(string actionName);
        List<string> GetAccessibleFeatures();
        List<string> GetAvailableActions();
    }
}
