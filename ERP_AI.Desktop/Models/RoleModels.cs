using System.ComponentModel.DataAnnotations;

namespace ERP_AI.Desktop.Models
{
    public class Role
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new();
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class Permission
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class CreateRoleRequest
    {
        [Required(ErrorMessage = "Role name is required")]
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new();
    }

    public class UpdateRoleRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<string>? Permissions { get; set; }
        public bool? IsActive { get; set; }
    }

    public class UserRole
    {
        public string UserId { get; set; } = string.Empty;
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
        public string AssignedBy { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class RolePermission
    {
        public string RoleId { get; set; } = string.Empty;
        public string PermissionId { get; set; } = string.Empty;
        public string PermissionName { get; set; } = string.Empty;
        public DateTime GrantedAt { get; set; }
        public string GrantedBy { get; set; } = string.Empty;
    }

    // Predefined roles and permissions
    public static class RoleNames
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Accountant = "Accountant";
        public const string User = "User";
        public const string ReadOnly = "ReadOnly";
    }

    public static class PermissionNames
    {
        // User Management
        public const string ViewUsers = "users.view";
        public const string CreateUsers = "users.create";
        public const string EditUsers = "users.edit";
        public const string DeleteUsers = "users.delete";
        public const string ManageUserRoles = "users.manage_roles";

        // Company Management
        public const string ViewCompany = "company.view";
        public const string EditCompany = "company.edit";
        public const string ManageCompanySettings = "company.manage_settings";

        // Financial Data
        public const string ViewFinancialData = "financial.view";
        public const string CreateFinancialData = "financial.create";
        public const string EditFinancialData = "financial.edit";
        public const string DeleteFinancialData = "financial.delete";
        public const string ApproveFinancialData = "financial.approve";

        // Reports
        public const string ViewReports = "reports.view";
        public const string CreateReports = "reports.create";
        public const string ExportReports = "reports.export";
        public const string ScheduleReports = "reports.schedule";

        // System Administration
        public const string ViewSystemLogs = "system.view_logs";
        public const string ManageSystemSettings = "system.manage_settings";
        public const string BackupData = "system.backup";
        public const string RestoreData = "system.restore";

        // Bank Reconciliation
        public const string ViewBankReconciliation = "bank_reconciliation.view";
        public const string CreateBankReconciliation = "bank_reconciliation.create";
        public const string EditBankReconciliation = "bank_reconciliation.edit";
        public const string ApproveBankReconciliation = "bank_reconciliation.approve";

        // Cash Flow Management
        public const string ViewCashFlow = "cash_flow.view";
        public const string CreateCashFlow = "cash_flow.create";
        public const string EditCashFlow = "cash_flow.edit";
        public const string ApproveCashFlow = "cash_flow.approve";

        // Budgeting
        public const string ViewBudget = "budget.view";
        public const string CreateBudget = "budget.create";
        public const string EditBudget = "budget.edit";
        public const string ApproveBudget = "budget.approve";
    }

    public static class FeatureNames
    {
        public const string UserManagement = "UserManagement";
        public const string CompanySettings = "CompanySettings";
        public const string FinancialData = "FinancialData";
        public const string Reports = "Reports";
        public const string SystemAdministration = "SystemAdministration";
        public const string BankReconciliation = "BankReconciliation";
        public const string CashFlowManagement = "CashFlowManagement";
        public const string Budgeting = "Budgeting";
        public const string AdvancedFeatures = "AdvancedFeatures";
    }

    public static class ActionNames
    {
        public const string Create = "Create";
        public const string Read = "Read";
        public const string Update = "Update";
        public const string Delete = "Delete";
        public const string Approve = "Approve";
        public const string Export = "Export";
        public const string Import = "Import";
        public const string Backup = "Backup";
        public const string Restore = "Restore";
    }
}
