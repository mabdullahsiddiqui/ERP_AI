using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERP_AI.Data;

namespace ERP_AI.Services
{
    public interface ICashFlowService
    {
        // Cash Flow Forecasting
        Task<CashFlowForecast> CreateForecastAsync(CashFlowForecast forecast);
        Task<CashFlowForecast> UpdateForecastAsync(CashFlowForecast forecast);
        Task<CashFlowForecast> GetForecastAsync(Guid forecastId);
        Task<List<CashFlowForecast>> GetForecastsAsync(Guid companyId);
        Task DeleteForecastAsync(Guid forecastId);
        Task<CashFlowForecast> CloneForecastAsync(Guid forecastId, string newName);
        
        // Cash Flow Projections
        Task<CashFlowProjection> AddProjectionAsync(Guid forecastId, CashFlowProjection projection);
        Task<CashFlowProjection> UpdateProjectionAsync(CashFlowProjection projection);
        Task DeleteProjectionAsync(Guid projectionId);
        Task<List<CashFlowProjection>> GetProjectionsAsync(Guid forecastId);
        Task<CashFlowProjection> GetProjectionAsync(Guid projectionId);
        
        // Scenario Management
        Task<CashFlowScenario> CreateScenarioAsync(Guid forecastId, CashFlowScenario scenario);
        Task<CashFlowScenario> UpdateScenarioAsync(CashFlowScenario scenario);
        Task DeleteScenarioAsync(Guid scenarioId);
        Task<List<CashFlowScenario>> GetScenariosAsync(Guid forecastId);
        Task<CashFlowScenario> GetScenarioAsync(Guid scenarioId);
        Task<CashFlowForecast> ApplyScenarioAsync(Guid forecastId, Guid scenarioId);
        
        // Forecast Analysis
        Task<ForecastAnalysis> AnalyzeForecastAsync(Guid forecastId);
        Task<ForecastComparison> CompareForecastsAsync(List<Guid> forecastIds);
        Task<ForecastAccuracy> CalculateForecastAccuracyAsync(Guid forecastId);
        Task<List<ForecastAlert>> GetForecastAlertsAsync(Guid forecastId);
        
        // Payment Scheduling
        Task<PaymentSchedule> CreatePaymentScheduleAsync(PaymentSchedule schedule);
        Task<PaymentSchedule> UpdatePaymentScheduleAsync(PaymentSchedule schedule);
        Task<PaymentSchedule> GetPaymentScheduleAsync(Guid scheduleId);
        Task<List<PaymentSchedule>> GetPaymentSchedulesAsync(Guid companyId);
        Task DeletePaymentScheduleAsync(Guid scheduleId);
        
        // Payment Schedule Items
        Task<PaymentScheduleItem> AddScheduleItemAsync(Guid scheduleId, PaymentScheduleItem item);
        Task<PaymentScheduleItem> UpdateScheduleItemAsync(PaymentScheduleItem item);
        Task DeleteScheduleItemAsync(Guid itemId);
        Task<List<PaymentScheduleItem>> GetScheduleItemsAsync(Guid scheduleId);
        Task<List<PaymentScheduleItem>> GetUpcomingPaymentsAsync(Guid companyId, int days);
        Task<List<PaymentScheduleItem>> GetOverduePaymentsAsync(Guid companyId);
        
        // Payment Optimization
        Task<PaymentOptimization> OptimizePaymentScheduleAsync(Guid scheduleId);
        Task<List<PaymentRecommendation>> GetPaymentRecommendationsAsync(Guid companyId);
        Task<PaymentSchedule> ReschedulePaymentsAsync(Guid scheduleId, RescheduleOptions options);
        
        // Working Capital Analysis
        Task<WorkingCapitalAnalysis> CreateWorkingCapitalAnalysisAsync(Guid companyId);
        Task<WorkingCapitalAnalysis> GetWorkingCapitalAnalysisAsync(Guid analysisId);
        Task<List<WorkingCapitalAnalysis>> GetWorkingCapitalHistoryAsync(Guid companyId);
        Task<WorkingCapitalTrend> CalculateWorkingCapitalTrendAsync(Guid companyId, int months);
        
        // Liquidity Planning
        Task<LiquidityPlan> CreateLiquidityPlanAsync(LiquidityPlan plan);
        Task<LiquidityPlan> UpdateLiquidityPlanAsync(LiquidityPlan plan);
        Task<LiquidityPlan> GetLiquidityPlanAsync(Guid planId);
        Task<List<LiquidityPlan>> GetLiquidityPlansAsync(Guid companyId);
        Task DeleteLiquidityPlanAsync(Guid planId);
        
        // Liquidity Actions
        Task<LiquidityAction> AddLiquidityActionAsync(Guid planId, LiquidityAction action);
        Task<LiquidityAction> UpdateLiquidityActionAsync(LiquidityAction action);
        Task DeleteLiquidityActionAsync(Guid actionId);
        Task<List<LiquidityAction>> GetLiquidityActionsAsync(Guid planId);
        Task<LiquidityAction> CompleteLiquidityActionAsync(Guid actionId, decimal actualAmount);
        
        // Cash Position Reporting
        Task<CashPosition> GetCurrentCashPositionAsync(Guid companyId);
        Task<CashPosition> CreateCashPositionSnapshotAsync(Guid companyId);
        Task<List<CashPosition>> GetCashPositionHistoryAsync(Guid companyId, DateTime startDate, DateTime endDate);
        Task<CashPositionTrend> AnalyzeCashPositionTrendAsync(Guid companyId, int months);
        
        // Payment Timing Analysis
        Task<PaymentTimingAnalysis> CreatePaymentTimingAnalysisAsync(Guid companyId, DateTime startDate, DateTime endDate);
        Task<PaymentTimingAnalysis> GetPaymentTimingAnalysisAsync(Guid analysisId);
        Task<List<PaymentTimingAnalysis>> GetPaymentTimingHistoryAsync(Guid companyId);
        Task<List<PaymentTimingRecommendation>> GetPaymentTimingRecommendationsAsync(Guid analysisId);
        
    // Cash Flow Reports
    Task<CashFlowAnalysisReport> GenerateCashFlowReportAsync(Guid companyId, DateTime startDate, DateTime endDate, string reportType);
    Task<byte[]> ExportCashFlowReportAsync(Guid reportId, string format);
    Task<List<CashFlowAnalysisReport>> GetCashFlowReportsAsync(Guid companyId);
        
        // Alerts and Notifications
        Task<List<CashFlowAlert>> GetCashFlowAlertsAsync(Guid companyId);
        Task<CashFlowAlert> CreateCashFlowAlertAsync(CashFlowAlert alert);
        Task<CashFlowAlert> UpdateCashFlowAlertAsync(CashFlowAlert alert);
        Task DeleteCashFlowAlertAsync(Guid alertId);
        
        // Data Integration
        Task<CashFlowForecast> ImportHistoricalDataAsync(Guid forecastId, List<HistoricalCashFlowData> data);
        Task<List<CashFlowProjection>> GenerateProjectionsFromHistoryAsync(Guid companyId, int months);
        Task<CashFlowForecast> AutoGenerateForecastAsync(Guid companyId, DateTime startDate, DateTime endDate);
    }

    // Supporting Models
    public class ForecastAnalysis
    {
        public Guid ForecastId { get; set; }
        public decimal TotalInflows { get; set; }
        public decimal TotalOutflows { get; set; }
        public decimal NetCashFlow { get; set; }
        public decimal EndingBalance { get; set; }
        public decimal MinBalance { get; set; }
        public decimal MaxBalance { get; set; }
        public DateTime MinBalanceDate { get; set; }
        public DateTime MaxBalanceDate { get; set; }
        public List<CashFlowCategory> Categories { get; set; } = new();
        public List<CashFlowTrend> Trends { get; set; } = new();
        public List<CashFlowRisk> Risks { get; set; } = new();
    }

    public class CashFlowCategory
    {
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
        public string FlowType { get; set; } = string.Empty;
    }

    public class CashFlowTrend
    {
        public string Period { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Change { get; set; }
        public decimal ChangePercentage { get; set; }
    }

    public class CashFlowRisk
    {
        public string RiskType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty; // Low, Medium, High, Critical
        public decimal Impact { get; set; }
        public string Mitigation { get; set; } = string.Empty;
    }

    public class ForecastComparison
    {
        public List<Guid> ForecastIds { get; set; } = new();
        public DateTime ComparisonDate { get; set; }
        public List<ForecastComparisonItem> Items { get; set; } = new();
        public ForecastComparisonSummary Summary { get; set; } = new();
    }

    public class ForecastComparisonItem
    {
        public string Metric { get; set; } = string.Empty;
        public Dictionary<Guid, decimal> Values { get; set; } = new();
        public decimal Variance { get; set; }
        public string BestForecast { get; set; } = string.Empty;
    }

    public class ForecastComparisonSummary
    {
        public string BestForecast { get; set; } = string.Empty;
        public string WorstForecast { get; set; } = string.Empty;
        public decimal AverageVariance { get; set; }
        public string Recommendation { get; set; } = string.Empty;
    }

    public class ForecastAccuracy
    {
        public Guid ForecastId { get; set; }
        public decimal OverallAccuracy { get; set; }
        public decimal InflowAccuracy { get; set; }
        public decimal OutflowAccuracy { get; set; }
        public decimal BalanceAccuracy { get; set; }
        public List<ForecastAccuracyPeriod> Periods { get; set; } = new();
    }

    public class ForecastAccuracyPeriod
    {
        public DateTime Period { get; set; }
        public decimal Forecasted { get; set; }
        public decimal Actual { get; set; }
        public decimal Accuracy { get; set; }
        public decimal Variance { get; set; }
    }

    public class ForecastAlert
    {
        public string AlertType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public DateTime AlertDate { get; set; }
        public string Action { get; set; } = string.Empty;
    }

    public class PaymentOptimization
    {
        public Guid ScheduleId { get; set; }
        public decimal CurrentCost { get; set; }
        public decimal OptimizedCost { get; set; }
        public decimal Savings { get; set; }
        public List<PaymentOptimizationItem> Recommendations { get; set; } = new();
    }

    public class PaymentOptimizationItem
    {
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Impact { get; set; }
        public string Priority { get; set; } = string.Empty;
    }

    public class PaymentRecommendation
    {
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal PotentialSavings { get; set; }
        public string Implementation { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
    }

    public class RescheduleOptions
    {
        public DateTime NewStartDate { get; set; }
        public string Frequency { get; set; } = string.Empty;
        public bool MaintainTotalAmount { get; set; } = true;
        public bool AvoidWeekends { get; set; } = true;
        public bool AvoidHolidays { get; set; } = true;
    }

    public class CashPositionTrend
    {
        public Guid CompanyId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal StartingBalance { get; set; }
        public decimal EndingBalance { get; set; }
        public decimal AverageBalance { get; set; }
        public decimal MinBalance { get; set; }
        public decimal MaxBalance { get; set; }
        public List<CashPositionTrendPoint> Points { get; set; } = new();
    }

    public class CashPositionTrendPoint
    {
        public DateTime Date { get; set; }
        public decimal Balance { get; set; }
        public decimal Change { get; set; }
        public decimal ChangePercentage { get; set; }
    }

    public class CashFlowAnalysisReport
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CompanyId { get; set; }
        public string ReportType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public string GeneratedBy { get; set; } = string.Empty;
        public string ReportData { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string Format { get; set; } = "PDF";
    }

    public class CashFlowAlert
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CompanyId { get; set; }
        public string AlertType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }
        public bool IsRead { get; set; } = false;
        public string Action { get; set; } = string.Empty;
    }

    public class HistoricalCashFlowData
    {
        public DateTime Date { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string FlowType { get; set; } = string.Empty;
    }
}
