using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ERP_AI.Core;

namespace ERP_AI.Data
{
    // Budgeting & Forecasting Models
    public class Budget : BaseEntity
    {
        public Guid CompanyId { get; set; }
        public Company Company { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string BudgetType { get; set; } = "Annual"; // Annual, Quarterly, Monthly, Project
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "Draft"; // Draft, Active, Locked, Archived
        public string Currency { get; set; } = "USD";
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetIncome { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastModifiedAt { get; set; }
        public string LastModifiedBy { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public List<BudgetItem> Items { get; set; } = new();
        public List<BudgetVariance> Variances { get; set; } = new();
        public List<BudgetScenario> Scenarios { get; set; } = new();
    }

    public class BudgetItem : BaseEntity
    {
        public Guid BudgetId { get; set; }
        public Budget Budget { get; set; } = null!;
        public Guid? AccountId { get; set; }
        public Account? Account { get; set; }
        public string Category { get; set; } = string.Empty; // Revenue, Expense, Asset, Liability, Equity
        public string SubCategory { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal BudgetedAmount { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal Variance { get; set; }
        public decimal VariancePercentage { get; set; }
        public string Period { get; set; } = string.Empty; // Monthly, Quarterly, etc.
        public int PeriodNumber { get; set; } // 1-12 for monthly, 1-4 for quarterly
        public string BudgetMethod { get; set; } = "Fixed"; // Fixed, Variable, Percentage, Trend
        public decimal? PercentageOfRevenue { get; set; }
        public string Formula { get; set; } = string.Empty; // For calculated budgets
        public bool IsCalculated { get; set; } = false;
        public string Notes { get; set; } = string.Empty;
        public List<BudgetItemDetail> Details { get; set; } = new();
    }

    public class BudgetItemDetail : BaseEntity
    {
        public Guid BudgetItemId { get; set; }
        public BudgetItem BudgetItem { get; set; } = null!;
        public DateTime PeriodDate { get; set; }
        public decimal BudgetedAmount { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal Variance { get; set; }
        public decimal VariancePercentage { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class BudgetVariance : BaseEntity
    {
        public Guid BudgetId { get; set; }
        public Budget Budget { get; set; } = null!;
        public Guid? BudgetItemId { get; set; }
        public BudgetItem? BudgetItem { get; set; }
        public DateTime VarianceDate { get; set; }
        public string VarianceType { get; set; } = string.Empty; // Favorable, Unfavorable, Neutral
        public decimal VarianceAmount { get; set; }
        public decimal VariancePercentage { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Impact { get; set; } = string.Empty; // High, Medium, Low
        public string Action { get; set; } = string.Empty;
        public string Responsible { get; set; } = string.Empty;
        public DateTime? ActionDate { get; set; }
        public string Status { get; set; } = "Open"; // Open, InProgress, Resolved, Closed
    }

    public class BudgetScenario : BaseEntity
    {
        public Guid BudgetId { get; set; }
        public Budget Budget { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ScenarioType { get; set; } = "Optimistic"; // Optimistic, Realistic, Pessimistic, Custom
        public decimal Probability { get; set; } = 50; // 0-100%
        public decimal AdjustmentFactor { get; set; } = 1.0m;
        public string AdjustmentRules { get; set; } = string.Empty; // JSON rules
        public bool IsActive { get; set; } = true;
        public List<BudgetScenarioItem> Items { get; set; } = new();
    }

    public class BudgetScenarioItem : BaseEntity
    {
        public Guid ScenarioId { get; set; }
        public BudgetScenario Scenario { get; set; } = null!;
        public Guid? BudgetItemId { get; set; }
        public BudgetItem? BudgetItem { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal OriginalAmount { get; set; }
        public decimal AdjustedAmount { get; set; }
        public decimal AdjustmentFactor { get; set; }
        public string AdjustmentReason { get; set; } = string.Empty;
    }

    // Department/Project Budgeting
    public class DepartmentBudget : BaseEntity
    {
        public Guid CompanyId { get; set; }
        public Company Company { get; set; } = null!;
        public Guid? DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public Guid? ManagerId { get; set; }
        public string ManagerName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalBudget { get; set; }
        public decimal SpentAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public string Status { get; set; } = "Active"; // Active, Exceeded, Completed, Cancelled
        public string Notes { get; set; } = string.Empty;
        public List<DepartmentBudgetItem> Items { get; set; } = new();
    }

    public class DepartmentBudgetItem : BaseEntity
    {
        public Guid DepartmentBudgetId { get; set; }
        public DepartmentBudget DepartmentBudget { get; set; } = null!;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal BudgetedAmount { get; set; }
        public decimal SpentAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public string Status { get; set; } = "Active";
        public string Notes { get; set; } = string.Empty;
    }

    public class ProjectBudget : BaseEntity
    {
        public Guid CompanyId { get; set; }
        public Company Company { get; set; } = null!;
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid? ProjectManagerId { get; set; }
        public string ProjectManagerName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalBudget { get; set; }
        public decimal SpentAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public string Status { get; set; } = "Planning"; // Planning, Active, OnHold, Completed, Cancelled
        public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
        public string Notes { get; set; } = string.Empty;
        public List<ProjectBudgetItem> Items { get; set; } = new();
    }

    public class ProjectBudgetItem : BaseEntity
    {
        public Guid ProjectBudgetId { get; set; }
        public ProjectBudget ProjectBudget { get; set; } = null!;
        public string Category { get; set; } = string.Empty; // Labor, Materials, Equipment, Travel, etc.
        public string Description { get; set; } = string.Empty;
        public decimal BudgetedAmount { get; set; }
        public decimal SpentAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public string Status { get; set; } = "Active";
        public string Notes { get; set; } = string.Empty;
    }

    // Rolling Forecast
    public class RollingForecast : BaseEntity
    {
        public Guid CompanyId { get; set; }
        public Company Company { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ForecastHorizon { get; set; } = 12; // months
        public int UpdateFrequency { get; set; } = 1; // months
        public string Status { get; set; } = "Active"; // Active, Paused, Completed
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
        public string LastUpdatedBy { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public List<RollingForecastItem> Items { get; set; } = new();
    }

    public class RollingForecastItem : BaseEntity
    {
        public Guid RollingForecastId { get; set; }
        public RollingForecast RollingForecast { get; set; } = null!;
        public Guid? AccountId { get; set; }
        public Account? Account { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime ForecastDate { get; set; }
        public decimal ForecastedAmount { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal Variance { get; set; }
        public decimal VariancePercentage { get; set; }
        public string ForecastMethod { get; set; } = string.Empty; // Trend, Seasonal, Regression, etc.
        public decimal Confidence { get; set; } = 80; // 0-100%
        public string Notes { get; set; } = string.Empty;
    }

    // Budget Templates
    public class BudgetTemplate : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public string CompanySize { get; set; } = string.Empty; // Small, Medium, Large
        public string BudgetType { get; set; } = "Annual";
        public bool IsPublic { get; set; } = false;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<BudgetTemplateItem> Items { get; set; } = new();
    }

    public class BudgetTemplateItem : BaseEntity
    {
        public Guid TemplateId { get; set; }
        public BudgetTemplate Template { get; set; } = null!;
        public string Category { get; set; } = string.Empty;
        public string SubCategory { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal PercentageOfRevenue { get; set; }
        public string Formula { get; set; } = string.Empty;
        public bool IsRequired { get; set; } = true;
        public int Order { get; set; } = 0;
    }

    // Budget Alerts and Notifications
    public class BudgetAlert : BaseEntity
    {
        public Guid CompanyId { get; set; }
        public Company Company { get; set; } = null!;
        public Guid? BudgetId { get; set; }
        public Budget? Budget { get; set; }
        public Guid? BudgetItemId { get; set; }
        public BudgetItem? BudgetItem { get; set; }
        public string AlertType { get; set; } = string.Empty; // Exceeded, Approaching, Variance, etc.
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = "Medium"; // Low, Medium, High, Critical
        public decimal Threshold { get; set; }
        public decimal ActualValue { get; set; }
        public decimal Variance { get; set; }
        public DateTime AlertDate { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
        public string Action { get; set; } = string.Empty;
        public string Responsible { get; set; } = string.Empty;
    }

    // Budget Approval Workflow
    public class BudgetApproval : BaseEntity
    {
        public Guid BudgetId { get; set; }
        public Budget Budget { get; set; } = null!;
        public string ApprovalLevel { get; set; } = string.Empty; // Department, Manager, CFO, CEO
        public string ApproverId { get; set; } = string.Empty;
        public string ApproverName { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Delegated
        public DateTime? ApprovedAt { get; set; }
        public string Comments { get; set; } = string.Empty;
        public string RejectionReason { get; set; } = string.Empty;
        public int Order { get; set; } = 0;
    }

    // Budget Performance Metrics
    public class BudgetPerformance : BaseEntity
    {
        public Guid BudgetId { get; set; }
        public Budget Budget { get; set; } = null!;
        public DateTime Period { get; set; }
        public decimal TotalBudgeted { get; set; }
        public decimal TotalActual { get; set; }
        public decimal TotalVariance { get; set; }
        public decimal VariancePercentage { get; set; }
        public decimal RevenueAccuracy { get; set; }
        public decimal ExpenseAccuracy { get; set; }
        public int ItemsOverBudget { get; set; }
        public int ItemsUnderBudget { get; set; }
        public decimal LargestVariance { get; set; }
        public string LargestVarianceCategory { get; set; } = string.Empty;
        public string PerformanceGrade { get; set; } = string.Empty; // A, B, C, D, F
    }
}
