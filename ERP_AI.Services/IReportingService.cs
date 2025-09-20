using ERP_AI.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERP_AI.Services
{
    public interface IReportingService
    {
        Task<TrialBalanceReport> GenerateTrialBalanceAsync(DateTime asOfDate);
        Task<ProfitLossReport> GenerateProfitLossAsync(DateTime fromDate, DateTime toDate);
        Task<BalanceSheetReport> GenerateBalanceSheetAsync(DateTime asOfDate);
        Task<AgingReport> GenerateAgingReportAsync(DateTime asOfDate, string entityType);
        Task<CashFlowReport> GenerateCashFlowAsync(DateTime fromDate, DateTime toDate);
        Task<AccountActivityReport> GenerateAccountActivityAsync(Guid accountId, DateTime fromDate, DateTime toDate);
        Task<List<ReportTemplate>> GetAvailableReportsAsync();
        Task<string> ExportReportToPdfAsync(object reportData, string reportType, string outputPath);
        Task<string> ExportReportToExcelAsync(object reportData, string reportType, string outputPath);
    }

    public class TrialBalanceReport
    {
        public DateTime AsOfDate { get; set; }
        public List<TrialBalanceItem> Items { get; set; } = new();
        public decimal TotalDebits { get; set; }
        public decimal TotalCredits { get; set; }
        public bool IsBalanced => TotalDebits == TotalCredits;
    }

    public class TrialBalanceItem
    {
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public AccountType AccountType { get; set; }
        public decimal DebitBalance { get; set; }
        public decimal CreditBalance { get; set; }
    }

    public class ProfitLossReport
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetIncome => TotalRevenue - TotalExpenses;
        public List<ProfitLossItem> RevenueItems { get; set; } = new();
        public List<ProfitLossItem> ExpenseItems { get; set; } = new();
    }

    public class ProfitLossItem
    {
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class BalanceSheetReport
    {
        public DateTime AsOfDate { get; set; }
        public decimal TotalAssets { get; set; }
        public decimal TotalLiabilities { get; set; }
        public decimal TotalEquity { get; set; }
        public List<BalanceSheetItem> Assets { get; set; } = new();
        public List<BalanceSheetItem> Liabilities { get; set; } = new();
        public List<BalanceSheetItem> Equity { get; set; } = new();
        public bool IsBalanced => TotalAssets == (TotalLiabilities + TotalEquity);
    }

    public class BalanceSheetItem
    {
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class AgingReport
    {
        public DateTime AsOfDate { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public List<AgingItem> Items { get; set; } = new();
        public decimal TotalCurrent { get; set; }
        public decimal Total30Days { get; set; }
        public decimal Total60Days { get; set; }
        public decimal Total90Days { get; set; }
        public decimal TotalOver90Days { get; set; }
        public decimal GrandTotal { get; set; }
    }

    public class AgingItem
    {
        public string EntityName { get; set; } = string.Empty;
        public decimal Current { get; set; }
        public decimal Days30 { get; set; }
        public decimal Days60 { get; set; }
        public decimal Days90 { get; set; }
        public decimal Over90Days { get; set; }
        public decimal Total { get; set; }
    }

    public class CashFlowReport
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal OperatingCashFlow { get; set; }
        public decimal InvestingCashFlow { get; set; }
        public decimal FinancingCashFlow { get; set; }
        public decimal NetCashFlow => OperatingCashFlow + InvestingCashFlow + FinancingCashFlow;
        public List<CashFlowItem> OperatingItems { get; set; } = new();
        public List<CashFlowItem> InvestingItems { get; set; } = new();
        public List<CashFlowItem> FinancingItems { get; set; } = new();
    }

    public class CashFlowItem
    {
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class AccountActivityReport
    {
        public Guid AccountId { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal BeginningBalance { get; set; }
        public decimal EndingBalance { get; set; }
        public List<AccountActivityItem> Transactions { get; set; } = new();
    }

    public class AccountActivityItem
    {
        public DateTime Date { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }
    }

    public class ReportTemplate
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool RequiresDateRange { get; set; }
        public bool RequiresAsOfDate { get; set; }
        public List<string> Parameters { get; set; } = new();
    }
}
