using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ERP_AI.Core;

namespace ERP_AI.Data
{
    // Bank Statement Import Models
    public class BankStatement : BaseEntity
    {
        public Guid BankAccountId { get; set; }
        public BankAccount BankAccount { get; set; } = null!;
        public DateTime StatementDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public string ImportSource { get; set; } = string.Empty; // CSV, QIF, OFX, Manual
        public string FileName { get; set; } = string.Empty;
        public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Imported"; // Imported, Processing, Reconciled, Error
        public string Notes { get; set; } = string.Empty;
        public List<BankStatementItem> Items { get; set; } = new();
    }

    public class BankStatementItem : BaseEntity
    {
        public Guid BankStatementId { get; set; }
        public BankStatement BankStatement { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string TransactionType { get; set; } = string.Empty; // Debit, Credit
        public decimal RunningBalance { get; set; }
        public string Category { get; set; } = string.Empty;
        public bool IsMatched { get; set; } = false;
        public Guid? MatchedTransactionId { get; set; }
        public Transaction? MatchedTransaction { get; set; }
        public string MatchStatus { get; set; } = "Unmatched"; // Unmatched, AutoMatched, ManualMatched, Excluded
        public decimal MatchConfidence { get; set; } = 0; // 0-100% confidence in auto-match
        public string Notes { get; set; } = string.Empty;
    }

    // Bank Reconciliation Models
    public class BankReconciliation : BaseEntity
    {
        public Guid BankAccountId { get; set; }
        public BankAccount BankAccount { get; set; } = null!;
        public Guid BankStatementId { get; set; }
        public BankStatement BankStatement { get; set; } = null!;
        public DateTime ReconciliationDate { get; set; }
        public decimal BookBalance { get; set; }
        public decimal BankBalance { get; set; }
        public decimal OutstandingDeposits { get; set; }
        public decimal OutstandingChecks { get; set; }
        public decimal OtherAdjustments { get; set; }
        public decimal ReconciledBalance { get; set; }
        public string Status { get; set; } = "InProgress"; // InProgress, Reconciled, Discrepancy
        public string ReconciledBy { get; set; } = string.Empty;
        public DateTime? ReconciledAt { get; set; }
        public string Notes { get; set; } = string.Empty;
        public List<ReconciliationItem> Items { get; set; } = new();
    }

    public class ReconciliationItem : BaseEntity
    {
        public Guid ReconciliationId { get; set; }
        public BankReconciliation Reconciliation { get; set; } = null!;
        public Guid? TransactionId { get; set; }
        public Transaction? Transaction { get; set; }
        public Guid? StatementItemId { get; set; }
        public BankStatementItem? StatementItem { get; set; }
        public string ItemType { get; set; } = string.Empty; // Transaction, StatementItem, Adjustment
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Reconciled, Excluded
        public string ReconciliationMethod { get; set; } = string.Empty; // Auto, Manual, DragDrop
        public decimal MatchScore { get; set; } = 0;
        public string Notes { get; set; } = string.Empty;
    }

    // Outstanding Items Tracking
    public class OutstandingItem : BaseEntity
    {
        public Guid BankAccountId { get; set; }
        public BankAccount BankAccount { get; set; } = null!;
        public Guid? TransactionId { get; set; }
        public Transaction? Transaction { get; set; }
        public string ItemType { get; set; } = string.Empty; // Check, Deposit, Transfer
        public string CheckNumber { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Payee { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Outstanding"; // Outstanding, Cleared, Voided, Stopped
        public DateTime? ClearedDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        public int DaysOutstanding { get; set; }
        public bool IsStale { get; set; } = false; // For checks older than 6 months
    }

    // Import Templates and Rules
    public class ImportTemplate : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string FileFormat { get; set; } = string.Empty; // CSV, QIF, OFX
        public string Description { get; set; } = string.Empty;
        public string FieldMappings { get; set; } = string.Empty; // JSON mapping configuration
        public string DateFormat { get; set; } = string.Empty;
        public string DecimalSeparator { get; set; } = ".";
        public string ThousandSeparator { get; set; } = ",";
        public bool IsDefault { get; set; } = false;
        public string SampleData { get; set; } = string.Empty;
        public List<ImportRule> Rules { get; set; } = new();
    }

    public class ImportRule : BaseEntity
    {
        public Guid TemplateId { get; set; }
        public ImportTemplate Template { get; set; } = null!;
        public string RuleType { get; set; } = string.Empty; // Category, Description, Amount
        public string FieldName { get; set; } = string.Empty;
        public string Pattern { get; set; } = string.Empty; // Regex pattern
        public string Action { get; set; } = string.Empty; // SetCategory, SetDescription, etc.
        public string Value { get; set; } = string.Empty;
        public int Priority { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }

    // Matching Algorithms
    public class MatchingRule : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string RuleType { get; set; } = string.Empty; // Amount, Date, Description, Reference
        public string Pattern { get; set; } = string.Empty;
        public decimal Tolerance { get; set; } = 0; // For amount matching
        public int DateToleranceDays { get; set; } = 3;
        public decimal Weight { get; set; } = 1.0m; // Weight for scoring
        public bool IsActive { get; set; } = true;
        public int Priority { get; set; } = 0;
    }

    // Reconciliation Reports
    public class ReconciliationReport : BaseEntity
    {
        public Guid ReconciliationId { get; set; }
        public BankReconciliation Reconciliation { get; set; } = null!;
        public string ReportType { get; set; } = string.Empty; // Summary, Detail, Audit
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public string GeneratedBy { get; set; } = string.Empty;
        public string ReportData { get; set; } = string.Empty; // JSON report data
        public string FilePath { get; set; } = string.Empty;
        public string Format { get; set; } = "PDF"; // PDF, Excel, CSV
    }

    // Audit Trail
    public class ReconciliationAudit : BaseEntity
    {
        public Guid ReconciliationId { get; set; }
        public BankReconciliation Reconciliation { get; set; } = null!;
        public string Action { get; set; } = string.Empty; // Created, Updated, Matched, Unmatched, Reconciled
        public string Description { get; set; } = string.Empty;
        public string OldValue { get; set; } = string.Empty;
        public string NewValue { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
    }

    // Enhanced Bank Account with reconciliation features
    public class BankAccountEnhanced : BankAccount
    {
        public decimal LastReconciledBalance { get; set; }
        public DateTime? LastReconciliationDate { get; set; }
        public string ReconciliationStatus { get; set; } = "NotStarted"; // NotStarted, InProgress, Reconciled, Overdue
        public int DaysSinceLastReconciliation { get; set; }
        public bool RequiresReconciliation { get; set; } = false;
        public decimal OutstandingChecksTotal { get; set; }
        public decimal OutstandingDepositsTotal { get; set; }
        public int OutstandingItemsCount { get; set; }
        public List<BankReconciliation> Reconciliations { get; set; } = new();
        public List<OutstandingItem> OutstandingItems { get; set; } = new();
    }
}
