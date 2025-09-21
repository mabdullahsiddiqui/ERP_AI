using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ERP_AI.Core;

namespace ERP_AI.Data
{
    // Cash Flow Management Models
    public class CashFlowForecast : BaseEntity
    {
        public Guid CompanyId { get; set; }
        public Company Company { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ForecastType { get; set; } = "Monthly"; // Daily, Weekly, Monthly, Quarterly
        public decimal StartingCashBalance { get; set; }
        public decimal ProjectedEndingBalance { get; set; }
        public string Status { get; set; } = "Draft"; // Draft, Active, Completed, Archived
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public List<CashFlowProjection> Projections { get; set; } = new();
        public List<CashFlowScenario> Scenarios { get; set; } = new();
    }

    public class CashFlowProjection : BaseEntity
    {
        public Guid ForecastId { get; set; }
        public CashFlowForecast Forecast { get; set; } = null!;
        public DateTime ProjectionDate { get; set; }
        public string Category { get; set; } = string.Empty; // Operating, Investing, Financing
        public string SubCategory { get; set; } = string.Empty; // Sales, Purchases, Loans, etc.
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string FlowType { get; set; } = "Inflow"; // Inflow, Outflow
        public decimal Probability { get; set; } = 100; // 0-100% probability
        public string Source { get; set; } = "Manual"; // Manual, Historical, Trend, Seasonal
        public bool IsRecurring { get; set; } = false;
        public string RecurrencePattern { get; set; } = string.Empty; // Daily, Weekly, Monthly, etc.
        public DateTime? RecurrenceEndDate { get; set; }
        public string Tags { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public List<CashFlowDependency> Dependencies { get; set; } = new();
    }

    public class CashFlowScenario : BaseEntity
    {
        public Guid ForecastId { get; set; }
        public CashFlowForecast Forecast { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ScenarioType { get; set; } = "Optimistic"; // Optimistic, Realistic, Pessimistic, Custom
        public decimal Probability { get; set; } = 50; // 0-100% probability of occurrence
        public decimal AdjustmentFactor { get; set; } = 1.0m; // Multiplier for amounts
        public string AdjustmentRules { get; set; } = string.Empty; // JSON rules for adjustments
        public bool IsActive { get; set; } = true;
        public List<CashFlowProjection> Projections { get; set; } = new();
    }

    public class CashFlowDependency : BaseEntity
    {
        public Guid ProjectionId { get; set; }
        public CashFlowProjection Projection { get; set; } = null!;
        public Guid DependentProjectionId { get; set; }
        public CashFlowProjection DependentProjection { get; set; } = null!;
        public string DependencyType { get; set; } = string.Empty; // Prerequisite, Conditional, Sequential
        public decimal DependencyFactor { get; set; } = 1.0m; // How much this affects the dependent projection
        public string Description { get; set; } = string.Empty;
    }

    // Payment Scheduling
    public class PaymentSchedule : BaseEntity
    {
        public Guid CompanyId { get; set; }
        public Company Company { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ScheduleType { get; set; } = "Fixed"; // Fixed, Variable, Conditional
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "USD";
        public string Status { get; set; } = "Active"; // Active, Paused, Completed, Cancelled
        public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
        public string PaymentMethod { get; set; } = string.Empty; // Check, ACH, Wire, Cash
        public string Notes { get; set; } = string.Empty;
        public List<PaymentScheduleItem> Items { get; set; } = new();
    }

    public class PaymentScheduleItem : BaseEntity
    {
        public Guid ScheduleId { get; set; }
        public PaymentSchedule Schedule { get; set; } = null!;
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Pending, Paid, Overdue, Cancelled
        public DateTime? PaidDate { get; set; }
        public string PaymentReference { get; set; } = string.Empty;
        public decimal PaidAmount { get; set; }
        public string Notes { get; set; } = string.Empty;
        public int DaysOverdue { get; set; }
        public bool IsCritical { get; set; } = false;
    }

    // Working Capital Analysis
    public class WorkingCapitalAnalysis : BaseEntity
    {
        public Guid CompanyId { get; set; }
        public Company Company { get; set; } = null!;
        public DateTime AnalysisDate { get; set; }
        public decimal CurrentAssets { get; set; }
        public decimal CurrentLiabilities { get; set; }
        public decimal WorkingCapital { get; set; }
        public decimal CurrentRatio { get; set; }
        public decimal QuickRatio { get; set; }
        public decimal CashRatio { get; set; }
        public decimal DaysSalesOutstanding { get; set; }
        public decimal DaysInventoryOutstanding { get; set; }
        public decimal DaysPayableOutstanding { get; set; }
        public decimal CashConversionCycle { get; set; }
        public string Analysis { get; set; } = string.Empty;
        public List<WorkingCapitalTrend> Trends { get; set; } = new();
    }

    public class WorkingCapitalTrend : BaseEntity
    {
        public Guid AnalysisId { get; set; }
        public WorkingCapitalAnalysis Analysis { get; set; } = null!;
        public DateTime PeriodDate { get; set; }
        public decimal WorkingCapital { get; set; }
        public decimal CurrentRatio { get; set; }
        public decimal QuickRatio { get; set; }
        public decimal CashRatio { get; set; }
        public decimal CashConversionCycle { get; set; }
    }

    // Liquidity Planning
    public class LiquidityPlan : BaseEntity
    {
        public Guid CompanyId { get; set; }
        public Company Company { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime PlanDate { get; set; }
        public DateTime HorizonDate { get; set; }
        public decimal CurrentLiquidity { get; set; }
        public decimal RequiredLiquidity { get; set; }
        public decimal LiquidityGap { get; set; }
        public string Status { get; set; } = "Draft"; // Draft, Active, Implemented, Archived
        public string PlanType { get; set; } = "Conservative"; // Conservative, Moderate, Aggressive
        public List<LiquidityAction> Actions { get; set; } = new();
        public List<LiquidityScenario> Scenarios { get; set; } = new();
    }

    public class LiquidityAction : BaseEntity
    {
        public Guid PlanId { get; set; }
        public LiquidityPlan Plan { get; set; } = null!;
        public string ActionType { get; set; } = string.Empty; // Increase, Decrease, Maintain
        public string Category { get; set; } = string.Empty; // Revenue, Expenses, Financing, Investment
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime TargetDate { get; set; }
        public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
        public string Status { get; set; } = "Planned"; // Planned, InProgress, Completed, Cancelled
        public string Responsible { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public decimal ActualAmount { get; set; }
        public DateTime? CompletedDate { get; set; }
    }

    public class LiquidityScenario : BaseEntity
    {
        public Guid PlanId { get; set; }
        public LiquidityPlan Plan { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ScenarioType { get; set; } = "Base"; // Base, Optimistic, Pessimistic, Stress
        public decimal Probability { get; set; } = 50;
        public decimal LiquidityImpact { get; set; }
        public string ImpactDescription { get; set; } = string.Empty;
        public List<LiquidityAction> Actions { get; set; } = new();
    }

    // Cash Position Reporting
    public class CashPosition : BaseEntity
    {
        public Guid CompanyId { get; set; }
        public Company Company { get; set; } = null!;
        public DateTime PositionDate { get; set; }
        public decimal TotalCash { get; set; }
        public decimal AvailableCash { get; set; }
        public decimal RestrictedCash { get; set; }
        public decimal CashEquivalents { get; set; }
        public decimal ShortTermInvestments { get; set; }
        public decimal TotalLiquidity { get; set; }
        public string Currency { get; set; } = "USD";
        public List<CashPositionDetail> Details { get; set; } = new();
    }

    public class CashPositionDetail : BaseEntity
    {
        public Guid PositionId { get; set; }
        public CashPosition Position { get; set; } = null!;
        public Guid? BankAccountId { get; set; }
        public BankAccount? BankAccount { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty; // Checking, Savings, Money Market, etc.
        public decimal Balance { get; set; }
        public decimal AvailableBalance { get; set; }
        public decimal PendingBalance { get; set; }
        public string Status { get; set; } = "Active"; // Active, Inactive, Closed
        public string Notes { get; set; } = string.Empty;
    }

    // Payment Timing Analysis
    public class PaymentTimingAnalysis : BaseEntity
    {
        public Guid CompanyId { get; set; }
        public Company Company { get; set; } = null!;
        public DateTime AnalysisDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalPayments { get; set; }
        public decimal AveragePaymentSize { get; set; }
        public int PaymentCount { get; set; }
        public decimal PaymentVelocity { get; set; } // Payments per day
        public List<PaymentTimingPattern> Patterns { get; set; } = new();
        public List<PaymentTimingRecommendation> Recommendations { get; set; } = new();
    }

    public class PaymentTimingPattern : BaseEntity
    {
        public Guid AnalysisId { get; set; }
        public PaymentTimingAnalysis Analysis { get; set; } = null!;
        public string PatternType { get; set; } = string.Empty; // DayOfWeek, DayOfMonth, Seasonal
        public string PatternValue { get; set; } = string.Empty; // Monday, 1st, Q1, etc.
        public decimal PaymentAmount { get; set; }
        public int PaymentCount { get; set; }
        public decimal Percentage { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class PaymentTimingRecommendation : BaseEntity
    {
        public Guid AnalysisId { get; set; }
        public PaymentTimingAnalysis Analysis { get; set; } = null!;
        public string RecommendationType { get; set; } = string.Empty; // Timing, Amount, Frequency
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = "Medium"; // Low, Medium, High
        public decimal PotentialSavings { get; set; }
        public string ImplementationNotes { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Pending, Implemented, Rejected
    }
}
