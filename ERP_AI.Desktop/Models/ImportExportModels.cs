using System;
using System.Collections.Generic;

namespace ERP_AI.Desktop.Services
{
    public class ImportRequest
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public Guid? TemplateId { get; set; }
        public DataMapping? Mapping { get; set; }
        public ImportSettings Settings { get; set; } = new ImportSettings();
    }

    public class ImportResult
    {
        public bool Success { get; set; }
        public int RecordsProcessed { get; set; }
        public int RecordsImported { get; set; }
        public int RecordsSkipped { get; set; }
        public int RecordsFailed { get; set; }
        public List<ImportError> Errors { get; set; } = new List<ImportError>();
        public string Message { get; set; } = string.Empty;
    }

    public class ImportError
    {
        public int RowNumber { get; set; }
        public string Field { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string Severity { get; set; } = "Error";
    }

    public class ExportRequest
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public List<string> Fields { get; set; } = new List<string>();
        public ExportSettings Settings { get; set; } = new ExportSettings();
    }

    public class ExportResult
    {
        public bool Success { get; set; }
        public int RecordsExported { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class ImportTemplate
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public DataMapping Mapping { get; set; } = new DataMapping();
        public ImportSettings Settings { get; set; } = new ImportSettings();
        public DateTime CreatedDate { get; set; }
    }

    public class ExportTemplate
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public List<string> Fields { get; set; } = new List<string>();
        public ExportSettings Settings { get; set; } = new ExportSettings();
        public DateTime CreatedDate { get; set; }
    }

    public class DataMapping
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public List<FieldMapping> FieldMappings { get; set; } = new List<FieldMapping>();
        public DateTime CreatedDate { get; set; }
    }

    public class FieldMapping
    {
        public string SourceField { get; set; } = string.Empty;
        public string TargetField { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public string Transformation { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
    }

    public class DataMappingRequest
    {
        public string EntityType { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public List<string> SourceFields { get; set; } = new List<string>();
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<ValidationError> Errors { get; set; } = new List<ValidationError>();
        public List<ValidationWarning> Warnings { get; set; } = new List<ValidationWarning>();
    }

    public class ValidationError
    {
        public string Field { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int RowNumber { get; set; }
    }

    public class ValidationWarning
    {
        public string Field { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int RowNumber { get; set; }
    }

    public class ImportPreview
    {
        public List<Dictionary<string, object>> SampleData { get; set; } = new List<Dictionary<string, object>>();
        public List<string> AvailableFields { get; set; } = new List<string>();
        public int TotalRows { get; set; }
        public List<ValidationError> ValidationErrors { get; set; } = new List<ValidationError>();
    }

    public class ExportPreview
    {
        public List<Dictionary<string, object>> SampleData { get; set; } = new List<Dictionary<string, object>>();
        public List<string> SelectedFields { get; set; } = new List<string>();
        public int TotalRecords { get; set; }
        public long EstimatedFileSize { get; set; }
    }

    public class ImportHistory
    {
        public List<ImportHistoryItem> Items { get; set; } = new List<ImportHistoryItem>();
        public int TotalImports { get; set; }
        public DateTime LastImportDate { get; set; }
    }

    public class ImportHistoryItem
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public DateTime ImportDate { get; set; }
        public int RecordsImported { get; set; }
        public bool Success { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ExportHistory
    {
        public List<ExportHistoryItem> Items { get; set; } = new List<ExportHistoryItem>();
        public int TotalExports { get; set; }
        public DateTime LastExportDate { get; set; }
    }

    public class ExportHistoryItem
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public DateTime ExportDate { get; set; }
        public int RecordsExported { get; set; }
        public bool Success { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ImportSettings
    {
        public bool SkipEmptyRows { get; set; } = true;
        public bool SkipInvalidRows { get; set; } = false;
        public bool ValidateData { get; set; } = true;
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string NumberFormat { get; set; } = "en-US";
        public bool CreateMissingRecords { get; set; } = false;
    }

    public class ExportSettings
    {
        public bool IncludeHeaders { get; set; } = true;
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string NumberFormat { get; set; } = "en-US";
        public bool FormatNumbers { get; set; } = true;
        public string Delimiter { get; set; } = ",";
        public string QuoteCharacter { get; set; } = "\"";
    }

    public class ScheduledJob
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime ScheduledTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public object Request { get; set; } = new object();
    }
}
