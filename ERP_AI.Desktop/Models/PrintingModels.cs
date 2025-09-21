using System;
using System.Collections.Generic;

namespace ERP_AI.Desktop.Services
{
    public class PrintSettings
    {
        public string PrinterName { get; set; } = string.Empty;
        public int Copies { get; set; } = 1;
        public bool Collate { get; set; } = true;
        public string PaperSize { get; set; } = "A4";
        public string Orientation { get; set; } = "Portrait";
        public bool ColorPrinting { get; set; } = true;
        public int Quality { get; set; } = 300;
        public bool Duplex { get; set; } = false;
        public string PageRange { get; set; } = "All";
        public bool FitToPage { get; set; } = true;
    }

    public class Printer
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public List<string> SupportedPaperSizes { get; set; } = new List<string>();
        public bool SupportsColor { get; set; }
        public bool SupportsDuplex { get; set; }
    }

    public class PrintJob
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public int Pages { get; set; }
        public PrintSettings Settings { get; set; } = new PrintSettings();
    }

    public class PrintJobStatus
    {
        public string Status { get; set; } = string.Empty;
        public int Progress { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
    }

    public class PrintPreview
    {
        public List<PrintPage> Pages { get; set; } = new List<PrintPage>();
        public int TotalPages { get; set; }
        public PrintSettings Settings { get; set; } = new PrintSettings();
    }

    public class PrintPage
    {
        public int PageNumber { get; set; }
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
        public double Width { get; set; }
        public double Height { get; set; }
    }

    public class PageSetup
    {
        public string PaperSize { get; set; } = "A4";
        public string Orientation { get; set; } = "Portrait";
        public double TopMargin { get; set; } = 1.0;
        public double BottomMargin { get; set; } = 1.0;
        public double LeftMargin { get; set; } = 1.0;
        public double RightMargin { get; set; } = 1.0;
        public string Header { get; set; } = string.Empty;
        public string Footer { get; set; } = string.Empty;
    }

    public class PrintStatistics
    {
        public int TotalJobs { get; set; }
        public int SuccessfulJobs { get; set; }
        public int FailedJobs { get; set; }
        public int TotalPages { get; set; }
        public DateTime LastPrintDate { get; set; }
        public TimeSpan TotalPrintTime { get; set; }
    }
}
