using ERP_AI.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text;

namespace ERP_AI.Services
{
    public class PdfService : IPdfService
    {
        public PdfService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<string> GenerateInvoicePdfAsync(Invoice invoice, string outputPath)
        {
            try
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(12));

                        page.Header()
                            .Text("INVOICE")
                            .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Column(x =>
                            {
                                x.Item().Text($"Invoice Number: {invoice.InvoiceNumber}")
                                    .FontSize(14).SemiBold();
                                x.Item().Text($"Invoice Date: {invoice.InvoiceDate:yyyy-MM-dd}");
                                x.Item().Text($"Due Date: {invoice.DueDate:yyyy-MM-dd}");
                                x.Item().Text($"Status: {invoice.Status}").FontColor(GetStatusColor(invoice.Status));

                                x.Item().PaddingTop(20).Text("Bill To:").SemiBold();
                                x.Item().Text($"{invoice.Customer?.Name ?? "N/A"}");
                                x.Item().Text($"{invoice.Customer?.Address ?? ""}");
                                x.Item().Text($"{invoice.Customer?.City ?? ""}, {invoice.Customer?.State ?? ""} {invoice.Customer?.ZipCode ?? ""}");
                                x.Item().Text($"{invoice.Customer?.Country ?? ""}");

                                x.Item().PaddingTop(20).Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(1);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Element(CellStyle).Text("Description").SemiBold();
                                        header.Cell().Element(CellStyle).Text("Qty").SemiBold();
                                        header.Cell().Element(CellStyle).Text("Price").SemiBold();
                                        header.Cell().Element(CellStyle).Text("Amount").SemiBold();
                                    });

                                    foreach (var item in invoice.Items)
                                    {
                                        table.Cell().Element(CellStyle).Text(item.Description);
                                        table.Cell().Element(CellStyle).Text(item.Quantity.ToString("N2"));
                                        table.Cell().Element(CellStyle).Text(item.UnitPrice.ToString("C"));
                                        table.Cell().Element(CellStyle).Text(item.Amount.ToString("C"));
                                    }
                                });

                                x.Item().AlignRight().Text(text =>
                                {
                                    text.Span("Subtotal: ").SemiBold();
                                    text.Span(invoice.Subtotal.ToString("C"));
                                });
                                x.Item().AlignRight().Text(text =>
                                {
                                    text.Span("Tax: ").SemiBold();
                                    text.Span(invoice.TaxAmount.ToString("C"));
                                });
                                x.Item().AlignRight().Text(text =>
                                {
                                    text.Span("Total: ").SemiBold().FontSize(14);
                                    text.Span(invoice.Total.ToString("C")).FontSize(14);
                                });
                                x.Item().AlignRight().Text(text =>
                                {
                                    text.Span("Paid: ").SemiBold();
                                    text.Span(invoice.PaidAmount.ToString("C"));
                                });
                                x.Item().AlignRight().Text(text =>
                                {
                                    text.Span("Balance: ").SemiBold().FontColor(Colors.Red.Medium);
                                    text.Span(invoice.Balance.ToString("C")).FontColor(Colors.Red.Medium);
                                });

                                if (!string.IsNullOrEmpty(invoice.Notes))
                                {
                                    x.Item().PaddingTop(20).Text("Notes:").SemiBold();
                                    x.Item().Text(invoice.Notes);
                                }
                            });

                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Page ");
                                x.CurrentPageNumber();
                            });
                    });
                });

                document.GeneratePdf(outputPath);
                return outputPath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating invoice PDF: {ex.Message}", ex);
            }
        }

        public async Task<string> GenerateBillPdfAsync(Bill bill, string outputPath)
        {
            try
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(12));

                        page.Header()
                            .Text("BILL")
                            .SemiBold().FontSize(20).FontColor(Colors.Orange.Medium);

                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Column(x =>
                            {
                                x.Item().Text($"Bill Number: {bill.BillNumber}")
                                    .FontSize(14).SemiBold();
                                x.Item().Text($"Bill Date: {bill.BillDate:yyyy-MM-dd}");
                                x.Item().Text($"Due Date: {bill.DueDate:yyyy-MM-dd}");
                                x.Item().Text($"Status: {bill.Status}").FontColor(GetStatusColor(bill.Status));

                                x.Item().PaddingTop(20).Text("Vendor:").SemiBold();
                                x.Item().Text($"{bill.Vendor?.Name ?? "N/A"}");
                                x.Item().Text($"{bill.Vendor?.Address ?? ""}");
                                x.Item().Text($"{bill.Vendor?.City ?? ""}, {bill.Vendor?.State ?? ""} {bill.Vendor?.ZipCode ?? ""}");
                                x.Item().Text($"{bill.Vendor?.Country ?? ""}");

                                x.Item().PaddingTop(20).Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(1);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Element(CellStyle).Text("Description").SemiBold();
                                        header.Cell().Element(CellStyle).Text("Qty").SemiBold();
                                        header.Cell().Element(CellStyle).Text("Price").SemiBold();
                                        header.Cell().Element(CellStyle).Text("Amount").SemiBold();
                                    });

                                    foreach (var item in bill.Items)
                                    {
                                        table.Cell().Element(CellStyle).Text(item.Description);
                                        table.Cell().Element(CellStyle).Text(item.Quantity.ToString("N2"));
                                        table.Cell().Element(CellStyle).Text(item.UnitPrice.ToString("C"));
                                        table.Cell().Element(CellStyle).Text(item.Amount.ToString("C"));
                                    }
                                });

                                x.Item().AlignRight().Text(text =>
                                {
                                    text.Span("Subtotal: ").SemiBold();
                                    text.Span(bill.Subtotal.ToString("C"));
                                });
                                x.Item().AlignRight().Text(text =>
                                {
                                    text.Span("Tax: ").SemiBold();
                                    text.Span(bill.TaxAmount.ToString("C"));
                                });
                                x.Item().AlignRight().Text(text =>
                                {
                                    text.Span("Total: ").SemiBold().FontSize(14);
                                    text.Span(bill.Total.ToString("C")).FontSize(14);
                                });
                                x.Item().AlignRight().Text(text =>
                                {
                                    text.Span("Paid: ").SemiBold();
                                    text.Span(bill.PaidAmount.ToString("C"));
                                });
                                x.Item().AlignRight().Text(text =>
                                {
                                    text.Span("Balance: ").SemiBold().FontColor(Colors.Red.Medium);
                                    text.Span(bill.Balance.ToString("C")).FontColor(Colors.Red.Medium);
                                });

                                if (!string.IsNullOrEmpty(bill.Notes))
                                {
                                    x.Item().PaddingTop(20).Text("Notes:").SemiBold();
                                    x.Item().Text(bill.Notes);
                                }
                            });

                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Page ");
                                x.CurrentPageNumber();
                            });
                    });
                });

                document.GeneratePdf(outputPath);
                return outputPath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating bill PDF: {ex.Message}", ex);
            }
        }

        public async Task<string> GenerateReportPdfAsync(string reportType, object data, string outputPath)
        {
            try
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(12));

                        page.Header()
                            .Text(reportType.ToUpper())
                            .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Column(x =>
                            {
                                x.Item().Text($"Report Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                                    .FontSize(10).FontColor(Colors.Grey.Medium);

                                // Add report-specific content based on reportType
                                x.Item().PaddingTop(20).Text("Report data would be displayed here based on the report type.");
                            });

                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Page ");
                                x.CurrentPageNumber();
                            });
                    });
                });

                document.GeneratePdf(outputPath);
                return outputPath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating report PDF: {ex.Message}", ex);
            }
        }

        public async Task<byte[]> GenerateInvoicePdfBytesAsync(Invoice invoice)
        {
            using var stream = new MemoryStream();
            var document = CreateInvoiceDocument(invoice);
            document.GeneratePdf(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> GenerateBillPdfBytesAsync(Bill bill)
        {
            using var stream = new MemoryStream();
            var document = CreateBillDocument(bill);
            document.GeneratePdf(stream);
            return stream.ToArray();
        }

        private Document CreateInvoiceDocument(Invoice invoice)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .Text("INVOICE")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Item().Text($"Invoice Number: {invoice.InvoiceNumber}")
                                .FontSize(14).SemiBold();
                            x.Item().Text($"Invoice Date: {invoice.InvoiceDate:yyyy-MM-dd}");
                            x.Item().Text($"Due Date: {invoice.DueDate:yyyy-MM-dd}");
                            x.Item().Text($"Status: {invoice.Status}").FontColor(GetStatusColor(invoice.Status));

                            x.Item().PaddingTop(20).Text("Bill To:").SemiBold();
                            x.Item().Text($"{invoice.Customer?.Name ?? "N/A"}");
                            x.Item().Text($"{invoice.Customer?.Address ?? ""}");
                            x.Item().Text($"{invoice.Customer?.City ?? ""}, {invoice.Customer?.State ?? ""} {invoice.Customer?.ZipCode ?? ""}");
                            x.Item().Text($"{invoice.Customer?.Country ?? ""}");

                            x.Item().PaddingTop(20).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Description").SemiBold();
                                    header.Cell().Element(CellStyle).Text("Qty").SemiBold();
                                    header.Cell().Element(CellStyle).Text("Price").SemiBold();
                                    header.Cell().Element(CellStyle).Text("Amount").SemiBold();
                                });

                                foreach (var item in invoice.Items)
                                {
                                    table.Cell().Element(CellStyle).Text(item.Description);
                                    table.Cell().Element(CellStyle).Text(item.Quantity.ToString("N2"));
                                    table.Cell().Element(CellStyle).Text(item.UnitPrice.ToString("C"));
                                    table.Cell().Element(CellStyle).Text(item.Amount.ToString("C"));
                                }
                            });

                            x.Item().AlignRight().Text(text =>
                            {
                                text.Span("Subtotal: ").SemiBold();
                                text.Span(invoice.Subtotal.ToString("C"));
                            });
                            x.Item().AlignRight().Text(text =>
                            {
                                text.Span("Tax: ").SemiBold();
                                text.Span(invoice.TaxAmount.ToString("C"));
                            });
                            x.Item().AlignRight().Text(text =>
                            {
                                text.Span("Total: ").SemiBold().FontSize(14);
                                text.Span(invoice.Total.ToString("C")).FontSize(14);
                            });
                            x.Item().AlignRight().Text(text =>
                            {
                                text.Span("Paid: ").SemiBold();
                                text.Span(invoice.PaidAmount.ToString("C"));
                            });
                            x.Item().AlignRight().Text(text =>
                            {
                                text.Span("Balance: ").SemiBold().FontColor(Colors.Red.Medium);
                                text.Span(invoice.Balance.ToString("C")).FontColor(Colors.Red.Medium);
                            });

                            if (!string.IsNullOrEmpty(invoice.Notes))
                            {
                                x.Item().PaddingTop(20).Text("Notes:").SemiBold();
                                x.Item().Text(invoice.Notes);
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                        });
                });
            });
        }

        private Document CreateBillDocument(Bill bill)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .Text("BILL")
                        .SemiBold().FontSize(20).FontColor(Colors.Orange.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Item().Text($"Bill Number: {bill.BillNumber}")
                                .FontSize(14).SemiBold();
                            x.Item().Text($"Bill Date: {bill.BillDate:yyyy-MM-dd}");
                            x.Item().Text($"Due Date: {bill.DueDate:yyyy-MM-dd}");
                            x.Item().Text($"Status: {bill.Status}").FontColor(GetStatusColor(bill.Status));

                            x.Item().PaddingTop(20).Text("Vendor:").SemiBold();
                            x.Item().Text($"{bill.Vendor?.Name ?? "N/A"}");
                            x.Item().Text($"{bill.Vendor?.Address ?? ""}");
                            x.Item().Text($"{bill.Vendor?.City ?? ""}, {bill.Vendor?.State ?? ""} {bill.Vendor?.ZipCode ?? ""}");
                            x.Item().Text($"{bill.Vendor?.Country ?? ""}");

                            x.Item().PaddingTop(20).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Description").SemiBold();
                                    header.Cell().Element(CellStyle).Text("Qty").SemiBold();
                                    header.Cell().Element(CellStyle).Text("Price").SemiBold();
                                    header.Cell().Element(CellStyle).Text("Amount").SemiBold();
                                });

                                foreach (var item in bill.Items)
                                {
                                    table.Cell().Element(CellStyle).Text(item.Description);
                                    table.Cell().Element(CellStyle).Text(item.Quantity.ToString("N2"));
                                    table.Cell().Element(CellStyle).Text(item.UnitPrice.ToString("C"));
                                    table.Cell().Element(CellStyle).Text(item.Amount.ToString("C"));
                                }
                            });

                            x.Item().AlignRight().Text(text =>
                            {
                                text.Span("Subtotal: ").SemiBold();
                                text.Span(bill.Subtotal.ToString("C"));
                            });
                            x.Item().AlignRight().Text(text =>
                            {
                                text.Span("Tax: ").SemiBold();
                                text.Span(bill.TaxAmount.ToString("C"));
                            });
                            x.Item().AlignRight().Text(text =>
                            {
                                text.Span("Total: ").SemiBold().FontSize(14);
                                text.Span(bill.Total.ToString("C")).FontSize(14);
                            });
                            x.Item().AlignRight().Text(text =>
                            {
                                text.Span("Paid: ").SemiBold();
                                text.Span(bill.PaidAmount.ToString("C"));
                            });
                            x.Item().AlignRight().Text(text =>
                            {
                                text.Span("Balance: ").SemiBold().FontColor(Colors.Red.Medium);
                                text.Span(bill.Balance.ToString("C")).FontColor(Colors.Red.Medium);
                            });

                            if (!string.IsNullOrEmpty(bill.Notes))
                            {
                                x.Item().PaddingTop(20).Text("Notes:").SemiBold();
                                x.Item().Text(bill.Notes);
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                        });
                });
            });
        }

        private static IContainer CellStyle(IContainer container)
        {
            return container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(8)
                .Background(Colors.White);
        }

        private static Color GetStatusColor(string status)
        {
            return status switch
            {
                "Draft" => Colors.Grey.Medium,
                "Sent" => Colors.Blue.Medium,
                "Paid" => Colors.Green.Medium,
                "Overdue" => Colors.Red.Medium,
                "Void" => Colors.Grey.Darken2,
                "Received" => Colors.Blue.Medium,
                _ => Colors.Black
            };
        }
    }
}
