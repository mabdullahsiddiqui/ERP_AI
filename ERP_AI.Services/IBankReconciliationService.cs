using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERP_AI.Data;

namespace ERP_AI.Services
{
    public interface IBankReconciliationService
    {
        // Bank Statement Import
        Task<BankStatement> ImportBankStatementAsync(string filePath, Guid bankAccountId, ImportTemplate template);
        Task<BankStatement> ImportCsvStatementAsync(string filePath, Guid bankAccountId, CsvImportOptions options);
        Task<BankStatement> ImportQifStatementAsync(string filePath, Guid bankAccountId);
        Task<BankStatement> ImportOfxStatementAsync(string filePath, Guid bankAccountId);
        Task<List<BankStatementItem>> ParseStatementItemsAsync(string content, ImportTemplate template);
        Task<ImportTemplate> CreateImportTemplateAsync(ImportTemplate template);
        Task<List<ImportTemplate>> GetImportTemplatesAsync();
        Task<ImportTemplate> GetImportTemplateAsync(Guid templateId);

        // Statement Processing
        Task<BankStatement> ProcessStatementAsync(Guid statementId);
        Task<List<BankStatementItem>> CategorizeStatementItemsAsync(Guid statementId);
        Task<BankStatement> ValidateStatementAsync(Guid statementId);
        Task<BankStatement> FinalizeStatementAsync(Guid statementId);

        // Transaction Matching
        Task<List<MatchResult>> AutoMatchTransactionsAsync(Guid statementId);
        Task<MatchResult> MatchTransactionAsync(Guid statementItemId, Guid transactionId);
        Task UnmatchTransactionAsync(Guid statementItemId);
        Task<List<MatchCandidate>> GetMatchCandidatesAsync(Guid statementItemId);
        Task<decimal> CalculateMatchScoreAsync(BankStatementItem item, Transaction transaction);
        Task<List<MatchingRule>> GetMatchingRulesAsync();
        Task<MatchingRule> CreateMatchingRuleAsync(MatchingRule rule);

        // Reconciliation Process
        Task<BankReconciliation> StartReconciliationAsync(Guid bankAccountId, Guid statementId);
        Task<BankReconciliation> AddReconciliationItemAsync(Guid reconciliationId, ReconciliationItem item);
        Task<BankReconciliation> RemoveReconciliationItemAsync(Guid reconciliationId, Guid itemId);
        Task<BankReconciliation> UpdateReconciliationItemAsync(Guid reconciliationId, Guid itemId, ReconciliationItem item);
        Task<BankReconciliation> CalculateReconciliationBalanceAsync(Guid reconciliationId);
        Task<BankReconciliation> CompleteReconciliationAsync(Guid reconciliationId, string notes);
        Task<BankReconciliation> GetReconciliationAsync(Guid reconciliationId);
        Task<List<BankReconciliation>> GetReconciliationsAsync(Guid bankAccountId);

        // Outstanding Items Management
        Task<List<OutstandingItem>> GetOutstandingItemsAsync(Guid bankAccountId);
        Task<OutstandingItem> CreateOutstandingItemAsync(OutstandingItem item);
        Task<OutstandingItem> UpdateOutstandingItemAsync(OutstandingItem item);
        Task DeleteOutstandingItemAsync(Guid itemId);
        Task<List<OutstandingItem>> GetStaleItemsAsync(Guid bankAccountId);
        Task MarkItemAsClearedAsync(Guid itemId, DateTime clearedDate);

        // Reconciliation Reports
        Task<ReconciliationReport> GenerateReconciliationReportAsync(Guid reconciliationId, string reportType);
        Task<byte[]> ExportReconciliationReportAsync(Guid reconciliationId, string format);
        Task<List<ReconciliationReport>> GetReconciliationReportsAsync(Guid reconciliationId);

        // Audit and History
        Task<List<ReconciliationAudit>> GetReconciliationAuditTrailAsync(Guid reconciliationId);
        Task<ReconciliationAudit> LogReconciliationActionAsync(Guid reconciliationId, string action, string description, string oldValue, string newValue);

        // Validation and Business Rules
        Task<ValidationResult> ValidateReconciliationAsync(Guid reconciliationId);
        Task<bool> CanReconcileAccountAsync(Guid bankAccountId);
        Task<List<string>> GetReconciliationWarningsAsync(Guid reconciliationId);
        Task<decimal> CalculateDiscrepancyAsync(Guid reconciliationId);

        // Bulk Operations
        Task<List<BankStatement>> BulkImportStatementsAsync(List<BulkImportRequest> requests);
        Task<List<BankReconciliation>> BulkStartReconciliationsAsync(List<Guid> bankAccountIds);
        Task<List<MatchResult>> BulkMatchTransactionsAsync(List<BulkMatchRequest> requests);
    }

    // Supporting Models
    public class CsvImportOptions
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string DecimalSeparator { get; set; } = ".";
        public string ThousandSeparator { get; set; } = ",";
        public bool HasHeader { get; set; } = true;
        public int DateColumn { get; set; } = 0;
        public int DescriptionColumn { get; set; } = 1;
        public int AmountColumn { get; set; } = 2;
        public int ReferenceColumn { get; set; } = -1;
        public int BalanceColumn { get; set; } = -1;
        public string CreditIndicator { get; set; } = "CR";
        public string DebitIndicator { get; set; } = "DR";
    }

    public class MatchResult
    {
        public Guid StatementItemId { get; set; }
        public Guid? TransactionId { get; set; }
        public decimal MatchScore { get; set; }
        public string MatchMethod { get; set; } = string.Empty; // Auto, Manual, DragDrop
        public bool IsExactMatch { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime MatchedAt { get; set; } = DateTime.UtcNow;
    }

    public class MatchCandidate
    {
        public Guid TransactionId { get; set; }
        public Transaction Transaction { get; set; } = null!;
        public decimal MatchScore { get; set; }
        public string MatchReason { get; set; } = string.Empty;
        public List<string> MatchFactors { get; set; } = new();
    }

    public class BulkImportRequest
    {
        public string FilePath { get; set; } = string.Empty;
        public Guid BankAccountId { get; set; }
        public Guid? TemplateId { get; set; }
        public CsvImportOptions? CsvOptions { get; set; }
    }

    public class BulkMatchRequest
    {
        public Guid StatementItemId { get; set; }
        public Guid TransactionId { get; set; }
        public string MatchMethod { get; set; } = "Auto";
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public Dictionary<string, object> Data { get; set; } = new();
    }
}
