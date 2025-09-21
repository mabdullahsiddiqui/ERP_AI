using System;
using System.Collections.Generic;

namespace ERP_AI.Desktop.Services
{
    public class CompanyFile
    {
        public Guid Id { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyId { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime LastModified { get; set; }
        public DateTime LastAccessed { get; set; }
        public long FileSize { get; set; }
        public string Version { get; set; } = string.Empty;
        public bool IsDirty { get; set; }
        public bool IsReadOnly { get; set; }
        public string Description { get; set; } = string.Empty;
        public CompanySettings Settings { get; set; } = new CompanySettings();
    }

    public class CompanySettings
    {
        public string CompanyName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string TaxId { get; set; } = string.Empty;
        public string Currency { get; set; } = "USD";
        public string FiscalYearStart { get; set; } = "January";
        public bool UseMultiCurrency { get; set; }
        public bool EnableCloudSync { get; set; }
        public string CloudSyncUrl { get; set; } = string.Empty;
        public string CloudSyncApiKey { get; set; } = string.Empty;
    }

    public class RecentFile
    {
        public Guid Id { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public DateTime LastAccessed { get; set; }
        public int AccessCount { get; set; }
        public bool IsPinned { get; set; }
    }

    public class BackupFile
    {
        public Guid Id { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public long FileSize { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsAutomatic { get; set; }
        public string BackupType { get; set; } = "Full";
    }

    public class FileIntegrity
    {
        public bool IsValid { get; set; }
        public string Checksum { get; set; } = string.Empty;
        public DateTime LastChecked { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    public class FileRecovery
    {
        public bool IsRecoverable { get; set; }
        public string RecoveryPath { get; set; } = string.Empty;
        public DateTime RecoveryDate { get; set; }
        public string RecoveryMethod { get; set; } = string.Empty;
    }
}
