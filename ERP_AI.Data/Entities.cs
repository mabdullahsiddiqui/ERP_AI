using ERP_AI.Core;
using System;
using System.Collections.Generic;

namespace ERP_AI.Data
{
    public class Company : BaseEntity { public string Name { get; set; } = string.Empty; }
    public class User : BaseEntity { public string Username { get; set; } = string.Empty; }
    public enum AccountType { Asset, Liability, Equity, Revenue, Expense }

    public class Account : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public AccountType Type { get; set; }
        public Guid? ParentId { get; set; }
        public Account? Parent { get; set; }
        public List<Account> Children { get; set; } = new();
        public decimal Balance { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsSystem { get; set; } = false;
        public string Usage { get; set; } = string.Empty; // e.g. Cash, Bank, Receivable, Payable
    }
    public class Transaction : BaseEntity
    {
        public DateTime Date { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Draft"; // Draft, Posted, Void
        public decimal TotalDebits { get; set; }
        public decimal TotalCredits { get; set; }
        public bool IsBalanced => TotalDebits == TotalCredits;
        public ICollection<TransactionDetail> Details { get; set; } = new List<TransactionDetail>();
    }

    public class TransactionDetail : BaseEntity
    {
        public Guid TransactionId { get; set; }
        public Transaction? Transaction { get; set; }
        public Guid AccountId { get; set; }
        public Account? Account { get; set; }
        public decimal Amount { get; set; }
        public bool IsDebit { get; set; }
        public string Description { get; set; } = string.Empty;
    }
    public class Customer : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public decimal CreditLimit { get; set; }
        public decimal CurrentBalance { get; set; }
        public string PaymentTerms { get; set; } = string.Empty; // e.g., "Net 30", "Cash on Delivery"
        public bool IsActive { get; set; } = true;
        public string Notes { get; set; } = string.Empty;
    }

    public class Vendor : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string TaxId { get; set; } = string.Empty;
        public string PaymentTerms { get; set; } = string.Empty; // e.g., "Net 15", "Net 30"
        public bool IsActive { get; set; } = true;
        public string Notes { get; set; } = string.Empty;
    }

    public class Invoice : BaseEntity
    {
        public Guid CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; } = DateTime.Now;
        public DateTime DueDate { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal Balance { get; set; }
        public string Status { get; set; } = "Draft"; // Draft, Sent, Paid, Overdue, Void
        public string Notes { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = "USD";
        public decimal ExchangeRate { get; set; } = 1.0m;
        public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    }

    public class InvoiceItem : BaseEntity
    {
        public Guid InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
        public decimal TaxRate { get; set; }
        public string ItemCode { get; set; } = string.Empty;
    }

    public class Bill : BaseEntity
    {
        public Guid VendorId { get; set; }
        public Vendor? Vendor { get; set; }
        public string BillNumber { get; set; } = string.Empty;
        public DateTime BillDate { get; set; } = DateTime.Now;
        public DateTime DueDate { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal Balance { get; set; }
        public string Status { get; set; } = "Draft"; // Draft, Received, Paid, Overdue, Void
        public string Notes { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = "USD";
        public decimal ExchangeRate { get; set; } = 1.0m;
        public ICollection<BillItem> Items { get; set; } = new List<BillItem>();
    }

    public class BillItem : BaseEntity
    {
        public Guid BillId { get; set; }
        public Bill? Bill { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
        public decimal TaxRate { get; set; }
        public string ItemCode { get; set; } = string.Empty;
    }
    public class Payment : BaseEntity { public Guid AccountId { get; set; } public decimal Amount { get; set; } }
    public class BankAccount : BaseEntity { public string AccountNumber { get; set; } = string.Empty; }
    public class BankTransaction : BaseEntity { public Guid BankAccountId { get; set; } public decimal Amount { get; set; } }
    // Sync-related entities with comprehensive properties
    public class SyncLog : BaseEntity 
    { 
        public string Operation { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public string Details { get; set; } = string.Empty;
        public string Status { get; set; } = "Success";
        public DateTime SyncTime { get; set; } = DateTime.UtcNow;
    }
    
    public class ConflictResolution : BaseEntity 
    { 
        public string ConflictType { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public string LocalData { get; set; } = string.Empty;
        public string CloudData { get; set; } = string.Empty;
        public string Resolution { get; set; } = string.Empty;
        public DateTime ResolvedAt { get; set; }
        public string Status { get; set; } = "Pending";
        public string ResolvedBy { get; set; } = string.Empty;
    }
    
    public class CloudMapping : BaseEntity 
    { 
        public Guid LocalId { get; set; }
        public string CloudId { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public DateTime LastSynced { get; set; }
        public new string SyncStatus { get; set; } = "Pending";
    }
    
    public class SyncQueue : BaseEntity 
    { 
        public string EntityType { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public string Operation { get; set; } = string.Empty; // Create, Update, Delete
        public string EntityData { get; set; } = string.Empty;
        public int Priority { get; set; } = 1;
        public int RetryCount { get; set; } = 0;
        public DateTime NextRetry { get; set; }
        public string Status { get; set; } = "Pending";
        public string ErrorMessage { get; set; } = string.Empty;
    }
    
    public class SyncSettings : BaseEntity 
    { 
        public bool AutoSync { get; set; } = true;
        public DateTime LastSyncTime { get; set; }
        public int SyncIntervalMinutes { get; set; } = 30;
        public bool SyncOnStartup { get; set; } = true;
        public bool SyncOnShutdown { get; set; } = true;
        public string ServerUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string CompanyId { get; set; } = string.Empty;
        public bool EnableConflictResolution { get; set; } = true;
        public string ConflictResolutionStrategy { get; set; } = "UserChoice";
    }
}
