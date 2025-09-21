using System;
using System.Collections.Generic;

namespace ERP_AI.Desktop.Services
{
    // Financial Reports
    public class TrialBalanceReport
    {
        public DateTime AsOfDate { get; set; }
        public string Currency { get; set; } = string.Empty;
        public List<TrialBalanceItem> Items { get; set; } = new List<TrialBalanceItem>();
        public decimal TotalDebits { get; set; }
        public decimal TotalCredits { get; set; }
        public bool IsBalanced { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class TrialBalanceItem
    {
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public decimal DebitBalance { get; set; }
        public decimal CreditBalance { get; set; }
        public decimal NetBalance { get; set; }
    }

    public class ProfitLossReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Currency { get; set; } = string.Empty;
        public List<ProfitLossItem> Items { get; set; } = new List<ProfitLossItem>();
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetIncome { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal OperatingProfit { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class ProfitLossItem
    {
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal BudgetAmount { get; set; }
        public decimal Variance { get; set; }
        public decimal VariancePercentage { get; set; }
        public List<ProfitLossItem> SubItems { get; set; } = new List<ProfitLossItem>();
    }

    public class BalanceSheetReport
    {
        public DateTime AsOfDate { get; set; }
        public string Currency { get; set; } = string.Empty;
        public List<BalanceSheetItem> Assets { get; set; } = new List<BalanceSheetItem>();
        public List<BalanceSheetItem> Liabilities { get; set; } = new List<BalanceSheetItem>();
        public List<BalanceSheetItem> Equity { get; set; } = new List<BalanceSheetItem>();
        public decimal TotalAssets { get; set; }
        public decimal TotalLiabilities { get; set; }
        public decimal TotalEquity { get; set; }
        public bool IsBalanced { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class BalanceSheetItem
    {
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal CurrentAmount { get; set; }
        public decimal PreviousAmount { get; set; }
        public decimal Change { get; set; }
        public decimal ChangePercentage { get; set; }
        public List<BalanceSheetItem> SubItems { get; set; } = new List<BalanceSheetItem>();
    }

    public class CashFlowReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Currency { get; set; } = string.Empty;
        public CashFlowMethod Method { get; set; }
        public List<CashFlowItem> OperatingActivities { get; set; } = new List<CashFlowItem>();
        public List<CashFlowItem> InvestingActivities { get; set; } = new List<CashFlowItem>();
        public List<CashFlowItem> FinancingActivities { get; set; } = new List<CashFlowItem>();
        public decimal NetCashFlow { get; set; }
        public decimal BeginningCash { get; set; }
        public decimal EndingCash { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class CashFlowItem
    {
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Category { get; set; } = string.Empty;
        public bool IsInflow { get; set; }
    }

    public class AgingReport
    {
        public DateTime AsOfDate { get; set; }
        public string Currency { get; set; } = string.Empty;
        public AgingReportType Type { get; set; }
        public List<AgingItem> Items { get; set; } = new List<AgingItem>();
        public decimal TotalCurrent { get; set; }
        public decimal TotalOverdue { get; set; }
        public decimal GrandTotal { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class AgingItem
    {
        public string EntityName { get; set; } = string.Empty;
        public string EntityCode { get; set; } = string.Empty;
        public decimal Current { get; set; }
        public decimal Days30 { get; set; }
        public decimal Days60 { get; set; }
        public decimal Days90 { get; set; }
        public decimal Days120Plus { get; set; }
        public decimal Total { get; set; }
        public DateTime LastPaymentDate { get; set; }
    }

    // Management Reports
    public class SalesReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Currency { get; set; } = string.Empty;
        public List<SalesItem> Items { get; set; } = new List<SalesItem>();
        public decimal TotalSales { get; set; }
        public decimal TotalTax { get; set; }
        public decimal NetSales { get; set; }
        public int TotalInvoices { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class SalesItem
    {
        public string GroupKey { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal NetAmount { get; set; }
        public int InvoiceCount { get; set; }
    }

    public class PurchaseReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Currency { get; set; } = string.Empty;
        public List<PurchaseItem> Items { get; set; } = new List<PurchaseItem>();
        public decimal TotalPurchases { get; set; }
        public decimal TotalTax { get; set; }
        public decimal NetPurchases { get; set; }
        public int TotalBills { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class PurchaseItem
    {
        public string GroupKey { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal NetAmount { get; set; }
        public int BillCount { get; set; }
    }

    public class InventoryReport
    {
        public DateTime AsOfDate { get; set; }
        public string Currency { get; set; } = string.Empty;
        public List<InventoryItem> Items { get; set; } = new List<InventoryItem>();
        public decimal TotalValue { get; set; }
        public int TotalItems { get; set; }
        public int LowStockItems { get; set; }
        public int OutOfStockItems { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class InventoryItem
    {
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal QuantityOnHand { get; set; }
        public decimal QuantityReserved { get; set; }
        public decimal QuantityAvailable { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalValue { get; set; }
        public decimal ReorderLevel { get; set; }
        public bool IsLowStock { get; set; }
        public bool IsOutOfStock { get; set; }
    }

    public class CustomerReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Currency { get; set; } = string.Empty;
        public List<CustomerItem> Items { get; set; } = new List<CustomerItem>();
        public decimal TotalSales { get; set; }
        public decimal TotalOutstanding { get; set; }
        public int TotalCustomers { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class CustomerItem
    {
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public decimal TotalSales { get; set; }
        public decimal OutstandingBalance { get; set; }
        public int InvoiceCount { get; set; }
        public DateTime LastInvoiceDate { get; set; }
        public DateTime LastPaymentDate { get; set; }
    }

    public class VendorReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Currency { get; set; } = string.Empty;
        public List<VendorItem> Items { get; set; } = new List<VendorItem>();
        public decimal TotalPurchases { get; set; }
        public decimal TotalOutstanding { get; set; }
        public int TotalVendors { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class VendorItem
    {
        public string VendorCode { get; set; } = string.Empty;
        public string VendorName { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public decimal TotalPurchases { get; set; }
        public decimal OutstandingBalance { get; set; }
        public int BillCount { get; set; }
        public DateTime LastBillDate { get; set; }
        public DateTime LastPaymentDate { get; set; }
    }

    // Tax Reports
    public class TaxReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Currency { get; set; } = string.Empty;
        public List<TaxItem> Items { get; set; } = new List<TaxItem>();
        public decimal TotalTaxableAmount { get; set; }
        public decimal TotalTaxAmount { get; set; }
        public decimal TotalExemptAmount { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class TaxItem
    {
        public string TaxCode { get; set; } = string.Empty;
        public string TaxName { get; set; } = string.Empty;
        public decimal TaxRate { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ExemptAmount { get; set; }
        public int TransactionCount { get; set; }
    }

    public class VATReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Currency { get; set; } = string.Empty;
        public List<VATItem> Items { get; set; } = new List<VATItem>();
        public decimal TotalVATCollected { get; set; }
        public decimal TotalVATPaid { get; set; }
        public decimal NetVAT { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class VATItem
    {
        public string VATCode { get; set; } = string.Empty;
        public string VATName { get; set; } = string.Empty;
        public decimal VATRate { get; set; }
        public decimal SalesAmount { get; set; }
        public decimal VATCollected { get; set; }
        public decimal PurchaseAmount { get; set; }
        public decimal VATPaid { get; set; }
        public decimal NetVAT { get; set; }
    }

    public class PayrollReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Currency { get; set; } = string.Empty;
        public List<PayrollItem> Items { get; set; } = new List<PayrollItem>();
        public decimal TotalGrossPay { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalNetPay { get; set; }
        public decimal TotalTaxes { get; set; }
        public int TotalEmployees { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class PayrollItem
    {
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public decimal HoursWorked { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal GrossPay { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetPay { get; set; }
        public decimal Taxes { get; set; }
    }

    // Custom Report Builder
    public class CustomReport
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public CustomReportDefinition Definition { get; set; } = new CustomReportDefinition();
        public DateTime CreatedDate { get; set; }
        public DateTime LastModified { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
    }

    public class CustomReportDefinition
    {
        public string DataSource { get; set; } = string.Empty;
        public List<ReportField> Fields { get; set; } = new List<ReportField>();
        public List<ReportFilter> Filters { get; set; } = new List<ReportFilter>();
        public List<ReportSort> Sorts { get; set; } = new List<ReportSort>();
        public List<ReportGroup> Groups { get; set; } = new List<ReportGroup>();
        public ReportLayout Layout { get; set; } = new ReportLayout();
        public ReportParameters Parameters { get; set; } = new ReportParameters();
    }

    public class ReportField
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty;
        public bool IsVisible { get; set; } = true;
        public int Order { get; set; }
        public int Width { get; set; }
        public string Alignment { get; set; } = "Left";
    }

    public class ReportFilter
    {
        public string Field { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty;
        public object Value { get; set; } = new object();
        public string DataType { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
    }

    public class ReportSort
    {
        public string Field { get; set; } = string.Empty;
        public string Direction { get; set; } = "Asc";
        public int Order { get; set; }
    }

    public class ReportGroup
    {
        public string Field { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool ShowHeader { get; set; } = true;
        public bool ShowFooter { get; set; } = true;
        public int Order { get; set; }
    }

    public class ReportLayout
    {
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public bool ShowHeader { get; set; } = true;
        public bool ShowFooter { get; set; } = true;
        public bool ShowPageNumbers { get; set; } = true;
        public bool ShowDateGenerated { get; set; } = true;
        public string FontFamily { get; set; } = "Arial";
        public int FontSize { get; set; } = 10;
        public string Orientation { get; set; } = "Portrait";
        public string PaperSize { get; set; } = "A4";
    }

    public class ReportParameters
    {
        public Dictionary<string, object> Values { get; set; } = new Dictionary<string, object>();
    }

    // Report Templates
    public class ReportTemplate
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public CustomReportDefinition Definition { get; set; } = new CustomReportDefinition();
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
    }

    // Report Scheduling
    public class ScheduledReport
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public CustomReportDefinition Definition { get; set; } = new CustomReportDefinition();
        public ReportParameters Parameters { get; set; } = new ReportParameters();
        public string Schedule { get; set; } = string.Empty;
        public DateTime NextRun { get; set; }
        public DateTime LastRun { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<string> Recipients { get; set; } = new List<string>();
        public ExportFormat Format { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class ScheduledReportRequest
    {
        public string Name { get; set; } = string.Empty;
        public CustomReportDefinition Definition { get; set; } = new CustomReportDefinition();
        public ReportParameters Parameters { get; set; } = new ReportParameters();
        public string Schedule { get; set; } = string.Empty;
        public List<string> Recipients { get; set; } = new List<string>();
        public ExportFormat Format { get; set; }
    }

    // Export Options
    public class ExportOptions
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public bool IncludeHeaders { get; set; } = true;
        public bool IncludeFooters { get; set; } = true;
        public bool IncludePageNumbers { get; set; } = true;
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string NumberFormat { get; set; } = "N2";
    }

    public class PdfOptions
    {
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Keywords { get; set; } = string.Empty;
        public string Orientation { get; set; } = "Portrait";
        public string PaperSize { get; set; } = "A4";
        public int FontSize { get; set; } = 10;
        public string FontFamily { get; set; } = "Arial";
        public bool IncludeWatermark { get; set; }
        public string WatermarkText { get; set; } = string.Empty;
    }

    public class ExcelOptions
    {
        public string SheetName { get; set; } = "Report";
        public bool IncludeHeaders { get; set; } = true;
        public bool AutoFitColumns { get; set; } = true;
        public bool IncludeCharts { get; set; }
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string NumberFormat { get; set; } = "N2";
    }

    // Performance Metrics
    public class ReportPerformanceMetrics
    {
        public Guid ReportId { get; set; }
        public string ReportName { get; set; } = string.Empty;
        public TimeSpan AverageExecutionTime { get; set; }
        public TimeSpan LastExecutionTime { get; set; }
        public int TotalExecutions { get; set; }
        public int SuccessfulExecutions { get; set; }
        public int FailedExecutions { get; set; }
        public DateTime LastExecuted { get; set; }
        public long MemoryUsage { get; set; }
        public int CacheHits { get; set; }
        public int CacheMisses { get; set; }
    }

    public class ReportCacheStatus
    {
        public int TotalCachedReports { get; set; }
        public long TotalCacheSize { get; set; }
        public int CacheHits { get; set; }
        public int CacheMisses { get; set; }
        public double HitRatio { get; set; }
        public DateTime LastCleared { get; set; }
    }

    // Dashboard Integration
    public class DashboardWidget
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public WidgetDefinition Definition { get; set; } = new WidgetDefinition();
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsVisible { get; set; } = true;
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class WidgetDefinition
    {
        public string Title { get; set; } = string.Empty;
        public string DataSource { get; set; } = string.Empty;
        public string ChartType { get; set; } = string.Empty;
        public List<WidgetField> Fields { get; set; } = new List<WidgetField>();
        public List<WidgetFilter> Filters { get; set; } = new List<WidgetFilter>();
        public WidgetOptions Options { get; set; } = new WidgetOptions();
    }

    public class WidgetField
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // X, Y, Value, Label, etc.
    }

    public class WidgetFilter
    {
        public string Field { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty;
        public object Value { get; set; } = new object();
    }

    public class WidgetOptions
    {
        public bool ShowLegend { get; set; } = true;
        public bool ShowGrid { get; set; } = true;
        public bool ShowLabels { get; set; } = true;
        public string ColorScheme { get; set; } = "Default";
        public int RefreshInterval { get; set; } = 300; // seconds
    }

    public class DashboardData
    {
        public List<DashboardWidget> Widgets { get; set; } = new List<DashboardWidget>();
        public Dictionary<Guid, object> WidgetData { get; set; } = new Dictionary<Guid, object>();
        public DateTime LastUpdated { get; set; }
    }

    public class DashboardOptions
    {
        public List<Guid> WidgetIds { get; set; } = new List<Guid>();
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Currency { get; set; } = "USD";
        public bool IncludeRealTimeData { get; set; } = true;
    }

    // Base Report Result
    public class ReportResult
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public object Data { get; set; } = new object();
        public ReportMetadata Metadata { get; set; } = new ReportMetadata();
        public DateTime GeneratedAt { get; set; }
        public TimeSpan GenerationTime { get; set; }
    }

    public class ReportMetadata
    {
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Currency { get; set; } = "USD";
        public string GeneratedBy { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }
}
