using ERP_AI.Data;

namespace ERP_AI.Services
{
    public interface IPdfService
    {
        Task<string> GenerateInvoicePdfAsync(Invoice invoice, string outputPath);
        Task<string> GenerateBillPdfAsync(Bill bill, string outputPath);
        Task<string> GenerateReportPdfAsync(string reportType, object data, string outputPath);
        Task<byte[]> GenerateInvoicePdfBytesAsync(Invoice invoice);
        Task<byte[]> GenerateBillPdfBytesAsync(Bill bill);
    }
}
