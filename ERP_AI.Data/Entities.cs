using ERP_AI.Core;
using System;
using System.Collections.Generic;

namespace ERP_AI.Data
{
    public class Company : BaseEntity { public string Name { get; set; } = string.Empty; }
    public class User : BaseEntity { public string Username { get; set; } = string.Empty; }
    public class Account : BaseEntity { public string Code { get; set; } = string.Empty; public string Name { get; set; } = string.Empty; }
    public class Transaction : BaseEntity { public DateTime Date { get; set; } public ICollection<TransactionDetail> Details { get; set; } = new List<TransactionDetail>(); }
    public class TransactionDetail : BaseEntity { public Guid TransactionId { get; set; } public decimal Amount { get; set; } public bool IsDebit { get; set; } }
    public class Customer : BaseEntity { public string Name { get; set; } = string.Empty; }
    public class Vendor : BaseEntity { public string Name { get; set; } = string.Empty; }
    public class Invoice : BaseEntity { public Guid CustomerId { get; set; } public decimal Total { get; set; } }
    public class Bill : BaseEntity { public Guid VendorId { get; set; } public decimal Total { get; set; } }
    public class Payment : BaseEntity { public Guid AccountId { get; set; } public decimal Amount { get; set; } }
    public class BankAccount : BaseEntity { public string AccountNumber { get; set; } = string.Empty; }
    public class BankTransaction : BaseEntity { public Guid BankAccountId { get; set; } public decimal Amount { get; set; } }
    // Sync-related
    public class SyncLog : BaseEntity { public string Operation { get; set; } = string.Empty; }
    public class ConflictResolution : BaseEntity { public string ConflictType { get; set; } = string.Empty; }
    public class CloudMapping : BaseEntity { public Guid LocalId { get; set; } public string CloudId { get; set; } = string.Empty; }
    public class SyncQueue : BaseEntity { public string EntityType { get; set; } = string.Empty; }
    public class SyncSettings : BaseEntity { public bool AutoSync { get; set; } }
}
