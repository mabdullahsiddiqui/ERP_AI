using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERP_AI.Desktop.Services
{
    public interface IAdvancedReportingService
    {
        // Financial Reports
        Task<TrialBalanceReport> GenerateTrialBalanceAsync(TrialBalanceOptions options);
        Task<ProfitLossReport> GenerateProfitLossAsync(ProfitLossOptions options);
        Task<BalanceSheetReport> GenerateBalanceSheetAsync(BalanceSheetOptions options);
        Task<CashFlowReport> GenerateCashFlowAsync(CashFlowOptions options);
        Task<AgingReport> GenerateAgingReportAsync(AgingReportOptions options);

        // Management Reports
        Task<SalesReport> GenerateSalesReportAsync(SalesReportOptions options);
        Task<PurchaseReport> GeneratePurchaseReportAsync(PurchaseReportOptions options);
        Task<InventoryReport> GenerateInventoryReportAsync(InventoryReportOptions options);
        Task<CustomerReport> GenerateCustomerReportAsync(CustomerReportOptions options);
        Task<VendorReport> GenerateVendorReportAsync(VendorReportOptions options);

        // Tax Reports
        Task<TaxReport> GenerateTaxReportAsync(TaxReportOptions options);
        Task<VATReport> GenerateVATReportAsync(VATReportOptions options);
        Task<PayrollReport> GeneratePayrollReportAsync(PayrollReportOptions options);

        // Custom Report Builder
        Task<CustomReport> CreateCustomReportAsync(CustomReportDefinition definition);
        Task<CustomReport> UpdateCustomReportAsync(Guid reportId, CustomReportDefinition definition);
        Task<bool> DeleteCustomReportAsync(Guid reportId);
        Task<CustomReport> GetCustomReportAsync(Guid reportId);
        Task<List<CustomReport>> GetCustomReportsAsync();
        Task<CustomReport> ExecuteCustomReportAsync(Guid reportId, ReportParameters parameters);

        // Report Templates
        Task<List<ReportTemplate>> GetReportTemplatesAsync();
        Task<ReportTemplate> GetReportTemplateAsync(Guid templateId);
        Task<ReportTemplate> SaveReportTemplateAsync(ReportTemplate template);
        Task<bool> DeleteReportTemplateAsync(Guid templateId);

        // Report Scheduling
        Task<ScheduledReport> ScheduleReportAsync(ScheduledReportRequest request);
        Task<List<ScheduledReport>> GetScheduledReportsAsync();
        Task<bool> CancelScheduledReportAsync(Guid scheduleId);
        Task<bool> ExecuteScheduledReportAsync(Guid scheduleId);

        // Report Export
        Task<ExportResult> ExportReportAsync(ReportResult report, ExportFormat format, ExportOptions options);
        Task<byte[]> GenerateReportPdfAsync(ReportResult report, PdfOptions options);
        Task<byte[]> GenerateReportExcelAsync(ReportResult report, ExcelOptions options);

        // Report Performance
        Task<ReportPerformanceMetrics> GetReportPerformanceAsync(Guid reportId);
        Task<bool> OptimizeReportAsync(Guid reportId);
        Task<ReportCacheStatus> GetReportCacheStatusAsync();
        Task<bool> ClearReportCacheAsync();

        // Dashboard Integration
        Task<DashboardWidget> CreateDashboardWidgetAsync(WidgetDefinition definition);
        Task<List<DashboardWidget>> GetDashboardWidgetsAsync();
        Task<DashboardWidget> UpdateDashboardWidgetAsync(Guid widgetId, WidgetDefinition definition);
        Task<bool> DeleteDashboardWidgetAsync(Guid widgetId);
        Task<DashboardData> GetDashboardDataAsync(DashboardOptions options);
    }

    // Report Options
    public class TrialBalanceOptions
    {
        public DateTime AsOfDate { get; set; }
        public bool IncludeZeroBalances { get; set; } = false;
        public string Currency { get; set; } = "USD";
        public bool ShowSubAccounts { get; set; } = true;
        public List<string> AccountTypes { get; set; } = new List<string>();
    }

    public class ProfitLossOptions
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Currency { get; set; } = "USD";
        public bool ShowMonthlyBreakdown { get; set; } = true;
        public bool IncludeBudget { get; set; } = false;
        public bool ShowVariance { get; set; } = false;
    }

    public class BalanceSheetOptions
    {
        public DateTime AsOfDate { get; set; }
        public string Currency { get; set; } = "USD";
        public bool ShowSubAccounts { get; set; } = true;
        public bool IncludeBudget { get; set; } = false;
        public bool ShowVariance { get; set; } = false;
    }

    public class CashFlowOptions
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Currency { get; set; } = "USD";
        public CashFlowMethod Method { get; set; } = CashFlowMethod.Direct;
        public bool ShowMonthlyBreakdown { get; set; } = true;
    }

    public class AgingReportOptions
    {
        public DateTime AsOfDate { get; set; }
        public AgingReportType Type { get; set; } = AgingReportType.Customers;
        public List<int> AgingPeriods { get; set; } = new List<int> { 30, 60, 90, 120 };
        public string Currency { get; set; } = "USD";
        public bool IncludeZeroBalances { get; set; } = false;
    }

    public class SalesReportOptions
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Currency { get; set; } = "USD";
        public List<Guid> CustomerIds { get; set; } = new List<Guid>();
        public List<Guid> ProductIds { get; set; } = new List<Guid>();
        public SalesReportGroupBy GroupBy { get; set; } = SalesReportGroupBy.Customer;
        public bool IncludeTax { get; set; } = true;
    }

    public class PurchaseReportOptions
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Currency { get; set; } = "USD";
        public List<Guid> VendorIds { get; set; } = new List<Guid>();
        public List<Guid> ProductIds { get; set; } = new List<Guid>();
        public PurchaseReportGroupBy GroupBy { get; set; } = PurchaseReportGroupBy.Vendor;
        public bool IncludeTax { get; set; } = true;
    }

    public class InventoryReportOptions
    {
        public DateTime AsOfDate { get; set; }
        public string Currency { get; set; } = "USD";
        public List<Guid> ProductIds { get; set; } = new List<Guid>();
        public InventoryReportType Type { get; set; } = InventoryReportType.StockLevels;
        public bool IncludeZeroQuantities { get; set; } = false;
        public bool ShowValuation { get; set; } = true;
    }

    public class CustomerReportOptions
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Currency { get; set; } = "USD";
        public List<Guid> CustomerIds { get; set; } = new List<Guid>();
        public CustomerReportType Type { get; set; } = CustomerReportType.Summary;
        public bool IncludeContactInfo { get; set; } = true;
        public bool IncludePaymentHistory { get; set; } = true;
    }

    public class VendorReportOptions
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Currency { get; set; } = "USD";
        public List<Guid> VendorIds { get; set; } = new List<Guid>();
        public VendorReportType Type { get; set; } = VendorReportType.Summary;
        public bool IncludeContactInfo { get; set; } = true;
        public bool IncludePaymentHistory { get; set; } = true;
    }

    public class TaxReportOptions
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Currency { get; set; } = "USD";
        public TaxReportType Type { get; set; } = TaxReportType.Summary;
        public string TaxCode { get; set; } = string.Empty;
        public bool IncludeExemptions { get; set; } = true;
    }

    public class VATReportOptions
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Currency { get; set; } = "USD";
        public VATReportType Type { get; set; } = VATReportType.Summary;
        public decimal VATRate { get; set; } = 0.20m;
        public bool IncludeZeroRated { get; set; } = true;
    }

    public class PayrollReportOptions
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Currency { get; set; } = "USD";
        public List<Guid> EmployeeIds { get; set; } = new List<Guid>();
        public PayrollReportType Type { get; set; } = PayrollReportType.Summary;
        public bool IncludeDeductions { get; set; } = true;
        public bool IncludeBenefits { get; set; } = true;
    }

    // Enums
    public enum CashFlowMethod
    {
        Direct,
        Indirect
    }

    public enum AgingReportType
    {
        Customers,
        Vendors,
        Both
    }

    public enum SalesReportGroupBy
    {
        Customer,
        Product,
        Date,
        Salesperson
    }

    public enum PurchaseReportGroupBy
    {
        Vendor,
        Product,
        Date,
        Category
    }

    public enum InventoryReportType
    {
        StockLevels,
        Valuation,
        Movement,
        Reorder
    }

    public enum CustomerReportType
    {
        Summary,
        Detailed,
        Aging,
        PaymentHistory
    }

    public enum VendorReportType
    {
        Summary,
        Detailed,
        Aging,
        PaymentHistory
    }

    public enum TaxReportType
    {
        Summary,
        Detailed,
        ByCode,
        ByPeriod
    }

    public enum VATReportType
    {
        Summary,
        Detailed,
        ByRate,
        ByPeriod
    }

    public enum PayrollReportType
    {
        Summary,
        Detailed,
        ByEmployee,
        ByPeriod
    }

    public enum ExportFormat
    {
        PDF,
        Excel,
        CSV,
        XML,
        JSON
    }
}