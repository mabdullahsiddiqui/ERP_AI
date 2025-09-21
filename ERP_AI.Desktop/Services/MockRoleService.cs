using ERP_AI.Desktop.Models;

namespace ERP_AI.Desktop.Services
{
    public class MockRoleService : IRoleService
    {
        private readonly List<Role> _roles;
        private readonly List<Permission> _permissions;
        private readonly List<UserRole> _userRoles;
        private readonly List<RolePermission> _rolePermissions;
        private readonly IAuthenticationService _authService;

        public MockRoleService(IAuthenticationService authService)
        {
            _authService = authService;
            _roles = InitializeRoles();
            _permissions = InitializePermissions();
            _userRoles = new List<UserRole>();
            _rolePermissions = InitializeRolePermissions();
        }

        #region Role Management

        public async Task<List<Role>> GetRolesAsync()
        {
            await Task.Delay(100); // Simulate API call
            return _roles.ToList();
        }

        public async Task<Role?> GetRoleByIdAsync(string roleId)
        {
            await Task.Delay(100); // Simulate API call
            return _roles.FirstOrDefault(r => r.Id == roleId);
        }

        public async Task<Role> CreateRoleAsync(CreateRoleRequest request)
        {
            await Task.Delay(1000); // Simulate API call

            var role = new Role
            {
                Id = Guid.NewGuid().ToString(),
                Name = request.Name,
                Description = request.Description,
                Permissions = request.Permissions,
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                CreatedBy = _authService.CurrentUser?.Id ?? "system"
            };

            _roles.Add(role);
            return role;
        }

        public async Task<bool> UpdateRoleAsync(string roleId, UpdateRoleRequest request)
        {
            await Task.Delay(1000); // Simulate API call

            var role = _roles.FirstOrDefault(r => r.Id == roleId);
            if (role == null) return false;

            if (request.Name != null) role.Name = request.Name;
            if (request.Description != null) role.Description = request.Description;
            if (request.Permissions != null) role.Permissions = request.Permissions;
            if (request.IsActive.HasValue) role.IsActive = request.IsActive.Value;
            role.UpdatedAt = DateTime.Now;

            return true;
        }

        public async Task<bool> DeleteRoleAsync(string roleId)
        {
            await Task.Delay(1000); // Simulate API call

            var role = _roles.FirstOrDefault(r => r.Id == roleId);
            if (role == null) return false;

            // Soft delete
            role.IsActive = false;
            role.UpdatedAt = DateTime.Now;

            return true;
        }

        #endregion

        #region User Role Assignment

        public async Task<bool> AssignRoleToUserAsync(string userId, string roleId)
        {
            await Task.Delay(1000); // Simulate API call

            var existingAssignment = _userRoles.FirstOrDefault(ur => ur.UserId == userId && ur.RoleId == roleId);
            if (existingAssignment != null)
            {
                existingAssignment.IsActive = true;
                return true;
            }

            var role = _roles.FirstOrDefault(r => r.Id == roleId);
            if (role == null) return false;

            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = roleId,
                RoleName = role.Name,
                AssignedAt = DateTime.Now,
                AssignedBy = _authService.CurrentUser?.Id ?? "system",
                IsActive = true
            };

            _userRoles.Add(userRole);
            return true;
        }

        public async Task<bool> RemoveRoleFromUserAsync(string userId, string roleId)
        {
            await Task.Delay(1000); // Simulate API call

            var userRole = _userRoles.FirstOrDefault(ur => ur.UserId == userId && ur.RoleId == roleId);
            if (userRole == null) return false;

            userRole.IsActive = false;
            return true;
        }

        public async Task<List<Role>> GetUserRolesAsync(string userId)
        {
            await Task.Delay(100); // Simulate API call

            var userRoleIds = _userRoles
                .Where(ur => ur.UserId == userId && ur.IsActive)
                .Select(ur => ur.RoleId)
                .ToList();

            return _roles.Where(r => userRoleIds.Contains(r.Id) && r.IsActive).ToList();
        }

        public async Task<bool> HasRoleAsync(string userId, string roleName)
        {
            await Task.Delay(100); // Simulate API call

            var userRoles = await GetUserRolesAsync(userId);
            return userRoles.Any(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<bool> HasPermissionAsync(string userId, string permission)
        {
            await Task.Delay(100); // Simulate API call

            var userRoles = await GetUserRolesAsync(userId);
            return userRoles.Any(r => r.Permissions.Contains(permission));
        }

        #endregion

        #region Permission Management

        public async Task<List<Permission>> GetPermissionsAsync()
        {
            await Task.Delay(100); // Simulate API call
            return _permissions.ToList();
        }

        public async Task<List<Permission>> GetRolePermissionsAsync(string roleId)
        {
            await Task.Delay(100); // Simulate API call

            var role = _roles.FirstOrDefault(r => r.Id == roleId);
            if (role == null) return new List<Permission>();

            return _permissions.Where(p => role.Permissions.Contains(p.Name)).ToList();
        }

        public async Task<bool> AddPermissionToRoleAsync(string roleId, string permission)
        {
            await Task.Delay(1000); // Simulate API call

            var role = _roles.FirstOrDefault(r => r.Id == roleId);
            if (role == null) return false;

            if (!role.Permissions.Contains(permission))
            {
                role.Permissions.Add(permission);
                role.UpdatedAt = DateTime.Now;
            }

            return true;
        }

        public async Task<bool> RemovePermissionFromRoleAsync(string roleId, string permission)
        {
            await Task.Delay(1000); // Simulate API call

            var role = _roles.FirstOrDefault(r => r.Id == roleId);
            if (role == null) return false;

            role.Permissions.Remove(permission);
            role.UpdatedAt = DateTime.Now;

            return true;
        }

        #endregion

        #region Role-based UI Control

        public bool CanAccessFeature(string featureName)
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null) return false;

            // For demo purposes, allow access to most features
            // In a real implementation, this would check user permissions
            return featureName switch
            {
                FeatureNames.UserManagement => HasPermission(currentUser.Id, PermissionNames.ViewUsers),
                FeatureNames.CompanySettings => HasPermission(currentUser.Id, PermissionNames.ViewCompany),
                FeatureNames.FinancialData => HasPermission(currentUser.Id, PermissionNames.ViewFinancialData),
                FeatureNames.Reports => HasPermission(currentUser.Id, PermissionNames.ViewReports),
                FeatureNames.SystemAdministration => HasPermission(currentUser.Id, PermissionNames.ViewSystemLogs),
                FeatureNames.BankReconciliation => HasPermission(currentUser.Id, PermissionNames.ViewBankReconciliation),
                FeatureNames.CashFlowManagement => HasPermission(currentUser.Id, PermissionNames.ViewCashFlow),
                FeatureNames.Budgeting => HasPermission(currentUser.Id, PermissionNames.ViewBudget),
                FeatureNames.AdvancedFeatures => HasPermission(currentUser.Id, PermissionNames.ViewFinancialData),
                _ => true // Default allow for unknown features
            };
        }

        public bool CanPerformAction(string actionName)
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null) return false;

            // For demo purposes, allow most actions
            // In a real implementation, this would check specific permissions
            return actionName switch
            {
                ActionNames.Create => HasPermission(currentUser.Id, PermissionNames.CreateFinancialData),
                ActionNames.Read => HasPermission(currentUser.Id, PermissionNames.ViewFinancialData),
                ActionNames.Update => HasPermission(currentUser.Id, PermissionNames.EditFinancialData),
                ActionNames.Delete => HasPermission(currentUser.Id, PermissionNames.DeleteFinancialData),
                ActionNames.Approve => HasPermission(currentUser.Id, PermissionNames.ApproveFinancialData),
                ActionNames.Export => HasPermission(currentUser.Id, PermissionNames.ExportReports),
                ActionNames.Import => HasPermission(currentUser.Id, PermissionNames.CreateFinancialData),
                ActionNames.Backup => HasPermission(currentUser.Id, PermissionNames.BackupData),
                ActionNames.Restore => HasPermission(currentUser.Id, PermissionNames.RestoreData),
                _ => true // Default allow for unknown actions
            };
        }

        public List<string> GetAccessibleFeatures()
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null) return new List<string>();

            var features = new List<string>();
            var allFeatures = new[]
            {
                FeatureNames.UserManagement,
                FeatureNames.CompanySettings,
                FeatureNames.FinancialData,
                FeatureNames.Reports,
                FeatureNames.SystemAdministration,
                FeatureNames.BankReconciliation,
                FeatureNames.CashFlowManagement,
                FeatureNames.Budgeting,
                FeatureNames.AdvancedFeatures
            };

            foreach (var feature in allFeatures)
            {
                if (CanAccessFeature(feature))
                {
                    features.Add(feature);
                }
            }

            return features;
        }

        public List<string> GetAvailableActions()
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null) return new List<string>();

            var actions = new List<string>();
            var allActions = new[]
            {
                ActionNames.Create,
                ActionNames.Read,
                ActionNames.Update,
                ActionNames.Delete,
                ActionNames.Approve,
                ActionNames.Export,
                ActionNames.Import,
                ActionNames.Backup,
                ActionNames.Restore
            };

            foreach (var action in allActions)
            {
                if (CanPerformAction(action))
                {
                    actions.Add(action);
                }
            }

            return actions;
        }

        #endregion

        #region Private Helper Methods

        private bool HasPermission(string userId, string permission)
        {
            var userRoles = _userRoles
                .Where(ur => ur.UserId == userId && ur.IsActive)
                .Select(ur => ur.RoleId)
                .ToList();

            return _roles
                .Where(r => userRoles.Contains(r.Id) && r.IsActive)
                .Any(r => r.Permissions.Contains(permission));
        }

        private List<Role> InitializeRoles()
        {
            return new List<Role>
            {
                new Role
                {
                    Id = "1",
                    Name = RoleNames.SuperAdmin,
                    Description = "Full system access",
                    Permissions = new List<string>
                    {
                        PermissionNames.ViewUsers, PermissionNames.CreateUsers, PermissionNames.EditUsers, PermissionNames.DeleteUsers, PermissionNames.ManageUserRoles,
                        PermissionNames.ViewCompany, PermissionNames.EditCompany, PermissionNames.ManageCompanySettings,
                        PermissionNames.ViewFinancialData, PermissionNames.CreateFinancialData, PermissionNames.EditFinancialData, PermissionNames.DeleteFinancialData, PermissionNames.ApproveFinancialData,
                        PermissionNames.ViewReports, PermissionNames.CreateReports, PermissionNames.ExportReports, PermissionNames.ScheduleReports,
                        PermissionNames.ViewSystemLogs, PermissionNames.ManageSystemSettings, PermissionNames.BackupData, PermissionNames.RestoreData,
                        PermissionNames.ViewBankReconciliation, PermissionNames.CreateBankReconciliation, PermissionNames.EditBankReconciliation, PermissionNames.ApproveBankReconciliation,
                        PermissionNames.ViewCashFlow, PermissionNames.CreateCashFlow, PermissionNames.EditCashFlow, PermissionNames.ApproveCashFlow,
                        PermissionNames.ViewBudget, PermissionNames.CreateBudget, PermissionNames.EditBudget, PermissionNames.ApproveBudget
                    },
                    IsActive = true,
                    CreatedAt = DateTime.Now.AddDays(-30),
                    UpdatedAt = DateTime.Now.AddDays(-30),
                    CreatedBy = "system"
                },
                new Role
                {
                    Id = "2",
                    Name = RoleNames.Admin,
                    Description = "Administrative access",
                    Permissions = new List<string>
                    {
                        PermissionNames.ViewUsers, PermissionNames.CreateUsers, PermissionNames.EditUsers,
                        PermissionNames.ViewCompany, PermissionNames.EditCompany,
                        PermissionNames.ViewFinancialData, PermissionNames.CreateFinancialData, PermissionNames.EditFinancialData, PermissionNames.ApproveFinancialData,
                        PermissionNames.ViewReports, PermissionNames.CreateReports, PermissionNames.ExportReports,
                        PermissionNames.ViewBankReconciliation, PermissionNames.CreateBankReconciliation, PermissionNames.EditBankReconciliation, PermissionNames.ApproveBankReconciliation,
                        PermissionNames.ViewCashFlow, PermissionNames.CreateCashFlow, PermissionNames.EditCashFlow, PermissionNames.ApproveCashFlow,
                        PermissionNames.ViewBudget, PermissionNames.CreateBudget, PermissionNames.EditBudget, PermissionNames.ApproveBudget
                    },
                    IsActive = true,
                    CreatedAt = DateTime.Now.AddDays(-25),
                    UpdatedAt = DateTime.Now.AddDays(-25),
                    CreatedBy = "system"
                },
                new Role
                {
                    Id = "3",
                    Name = RoleNames.Manager,
                    Description = "Management access",
                    Permissions = new List<string>
                    {
                        PermissionNames.ViewUsers,
                        PermissionNames.ViewCompany,
                        PermissionNames.ViewFinancialData, PermissionNames.CreateFinancialData, PermissionNames.EditFinancialData,
                        PermissionNames.ViewReports, PermissionNames.CreateReports, PermissionNames.ExportReports,
                        PermissionNames.ViewBankReconciliation, PermissionNames.CreateBankReconciliation, PermissionNames.EditBankReconciliation,
                        PermissionNames.ViewCashFlow, PermissionNames.CreateCashFlow, PermissionNames.EditCashFlow,
                        PermissionNames.ViewBudget, PermissionNames.CreateBudget, PermissionNames.EditBudget
                    },
                    IsActive = true,
                    CreatedAt = DateTime.Now.AddDays(-20),
                    UpdatedAt = DateTime.Now.AddDays(-20),
                    CreatedBy = "system"
                },
                new Role
                {
                    Id = "4",
                    Name = RoleNames.Accountant,
                    Description = "Accounting access",
                    Permissions = new List<string>
                    {
                        PermissionNames.ViewFinancialData, PermissionNames.CreateFinancialData, PermissionNames.EditFinancialData,
                        PermissionNames.ViewReports, PermissionNames.CreateReports, PermissionNames.ExportReports,
                        PermissionNames.ViewBankReconciliation, PermissionNames.CreateBankReconciliation, PermissionNames.EditBankReconciliation,
                        PermissionNames.ViewCashFlow, PermissionNames.CreateCashFlow, PermissionNames.EditCashFlow,
                        PermissionNames.ViewBudget, PermissionNames.CreateBudget, PermissionNames.EditBudget
                    },
                    IsActive = true,
                    CreatedAt = DateTime.Now.AddDays(-15),
                    UpdatedAt = DateTime.Now.AddDays(-15),
                    CreatedBy = "system"
                },
                new Role
                {
                    Id = "5",
                    Name = RoleNames.User,
                    Description = "Basic user access",
                    Permissions = new List<string>
                    {
                        PermissionNames.ViewFinancialData,
                        PermissionNames.ViewReports,
                        PermissionNames.ViewBankReconciliation,
                        PermissionNames.ViewCashFlow,
                        PermissionNames.ViewBudget
                    },
                    IsActive = true,
                    CreatedAt = DateTime.Now.AddDays(-10),
                    UpdatedAt = DateTime.Now.AddDays(-10),
                    CreatedBy = "system"
                },
                new Role
                {
                    Id = "6",
                    Name = RoleNames.ReadOnly,
                    Description = "Read-only access",
                    Permissions = new List<string>
                    {
                        PermissionNames.ViewFinancialData,
                        PermissionNames.ViewReports,
                        PermissionNames.ViewBankReconciliation,
                        PermissionNames.ViewCashFlow,
                        PermissionNames.ViewBudget
                    },
                    IsActive = true,
                    CreatedAt = DateTime.Now.AddDays(-5),
                    UpdatedAt = DateTime.Now.AddDays(-5),
                    CreatedBy = "system"
                }
            };
        }

        private List<Permission> InitializePermissions()
        {
            return new List<Permission>
            {
                // User Management
                new Permission { Id = "1", Name = PermissionNames.ViewUsers, Description = "View users", Category = "User Management" },
                new Permission { Id = "2", Name = PermissionNames.CreateUsers, Description = "Create users", Category = "User Management" },
                new Permission { Id = "3", Name = PermissionNames.EditUsers, Description = "Edit users", Category = "User Management" },
                new Permission { Id = "4", Name = PermissionNames.DeleteUsers, Description = "Delete users", Category = "User Management" },
                new Permission { Id = "5", Name = PermissionNames.ManageUserRoles, Description = "Manage user roles", Category = "User Management" },

                // Company Management
                new Permission { Id = "6", Name = PermissionNames.ViewCompany, Description = "View company", Category = "Company Management" },
                new Permission { Id = "7", Name = PermissionNames.EditCompany, Description = "Edit company", Category = "Company Management" },
                new Permission { Id = "8", Name = PermissionNames.ManageCompanySettings, Description = "Manage company settings", Category = "Company Management" },

                // Financial Data
                new Permission { Id = "9", Name = PermissionNames.ViewFinancialData, Description = "View financial data", Category = "Financial Data" },
                new Permission { Id = "10", Name = PermissionNames.CreateFinancialData, Description = "Create financial data", Category = "Financial Data" },
                new Permission { Id = "11", Name = PermissionNames.EditFinancialData, Description = "Edit financial data", Category = "Financial Data" },
                new Permission { Id = "12", Name = PermissionNames.DeleteFinancialData, Description = "Delete financial data", Category = "Financial Data" },
                new Permission { Id = "13", Name = PermissionNames.ApproveFinancialData, Description = "Approve financial data", Category = "Financial Data" },

                // Reports
                new Permission { Id = "14", Name = PermissionNames.ViewReports, Description = "View reports", Category = "Reports" },
                new Permission { Id = "15", Name = PermissionNames.CreateReports, Description = "Create reports", Category = "Reports" },
                new Permission { Id = "16", Name = PermissionNames.ExportReports, Description = "Export reports", Category = "Reports" },
                new Permission { Id = "17", Name = PermissionNames.ScheduleReports, Description = "Schedule reports", Category = "Reports" },

                // System Administration
                new Permission { Id = "18", Name = PermissionNames.ViewSystemLogs, Description = "View system logs", Category = "System Administration" },
                new Permission { Id = "19", Name = PermissionNames.ManageSystemSettings, Description = "Manage system settings", Category = "System Administration" },
                new Permission { Id = "20", Name = PermissionNames.BackupData, Description = "Backup data", Category = "System Administration" },
                new Permission { Id = "21", Name = PermissionNames.RestoreData, Description = "Restore data", Category = "System Administration" },

                // Bank Reconciliation
                new Permission { Id = "22", Name = PermissionNames.ViewBankReconciliation, Description = "View bank reconciliation", Category = "Bank Reconciliation" },
                new Permission { Id = "23", Name = PermissionNames.CreateBankReconciliation, Description = "Create bank reconciliation", Category = "Bank Reconciliation" },
                new Permission { Id = "24", Name = PermissionNames.EditBankReconciliation, Description = "Edit bank reconciliation", Category = "Bank Reconciliation" },
                new Permission { Id = "25", Name = PermissionNames.ApproveBankReconciliation, Description = "Approve bank reconciliation", Category = "Bank Reconciliation" },

                // Cash Flow Management
                new Permission { Id = "26", Name = PermissionNames.ViewCashFlow, Description = "View cash flow", Category = "Cash Flow Management" },
                new Permission { Id = "27", Name = PermissionNames.CreateCashFlow, Description = "Create cash flow", Category = "Cash Flow Management" },
                new Permission { Id = "28", Name = PermissionNames.EditCashFlow, Description = "Edit cash flow", Category = "Cash Flow Management" },
                new Permission { Id = "29", Name = PermissionNames.ApproveCashFlow, Description = "Approve cash flow", Category = "Cash Flow Management" },

                // Budgeting
                new Permission { Id = "30", Name = PermissionNames.ViewBudget, Description = "View budget", Category = "Budgeting" },
                new Permission { Id = "31", Name = PermissionNames.CreateBudget, Description = "Create budget", Category = "Budgeting" },
                new Permission { Id = "32", Name = PermissionNames.EditBudget, Description = "Edit budget", Category = "Budgeting" },
                new Permission { Id = "33", Name = PermissionNames.ApproveBudget, Description = "Approve budget", Category = "Budgeting" }
            };
        }

        private List<RolePermission> InitializeRolePermissions()
        {
            var rolePermissions = new List<RolePermission>();
            var now = DateTime.Now;

            foreach (var role in _roles)
            {
                foreach (var permission in role.Permissions)
                {
                    rolePermissions.Add(new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permission,
                        PermissionName = permission,
                        GrantedAt = now,
                        GrantedBy = "system"
                    });
                }
            }

            return rolePermissions;
        }

        #endregion
    }
}