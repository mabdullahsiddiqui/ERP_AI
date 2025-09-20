using ERP_AI.Core;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using ERP_AI.Data;
using System.IO;
using Microsoft.Win32;
using ERP_AI.Services;

namespace ERP_AI.Desktop.ViewModels
{
    public class InvoiceViewModel : BaseViewModel
    {
        private readonly IPdfService _pdfService;

        public InvoiceViewModel(IPdfService pdfService)
        {
            _pdfService = pdfService;
        }

        public ObservableCollection<Invoice> Invoices { get; set; } = new();
        public ObservableCollection<Bill> Bills { get; set; } = new();
        public ObservableCollection<Customer> Customers { get; set; } = new();
        public ObservableCollection<Vendor> Vendors { get; set; } = new();
        public ObservableCollection<Payment> Payments { get; set; } = new();

        public Invoice? SelectedInvoice { get; set; }
        public Bill? SelectedBill { get; set; }
        public InvoiceItem? SelectedInvoiceItem { get; set; }
        public BillItem? SelectedBillItem { get; set; }

        // Invoice Commands
        public RelayCommand AddInvoiceCommand => new(_ => AddInvoice());
        public RelayCommand EditInvoiceCommand => new(_ => EditInvoice(), _ => SelectedInvoice != null);
        public RelayCommand DeleteInvoiceCommand => new(_ => DeleteInvoice(), _ => SelectedInvoice != null);
        public RelayCommand AddInvoiceItemCommand => new(_ => AddInvoiceItem(), _ => SelectedInvoice != null);
        public RelayCommand DeleteInvoiceItemCommand => new(_ => DeleteInvoiceItem(), _ => SelectedInvoiceItem != null);
        public RelayCommand GeneratePdfCommand => new(_ => GeneratePdf(), _ => SelectedInvoice != null);
        public RelayCommand TrackPaymentCommand => new(_ => TrackPayment(), _ => SelectedInvoice != null);

        // Bill Commands
        public RelayCommand AddBillCommand => new(_ => AddBill());
        public RelayCommand EditBillCommand => new(_ => EditBill(), _ => SelectedBill != null);
        public RelayCommand DeleteBillCommand => new(_ => DeleteBill(), _ => SelectedBill != null);
        public RelayCommand AddBillItemCommand => new(_ => AddBillItem(), _ => SelectedBill != null);
        public RelayCommand DeleteBillItemCommand => new(_ => DeleteBillItem(), _ => SelectedBillItem != null);
        public RelayCommand GenerateBillPdfCommand => new(_ => GenerateBillPdf(), _ => SelectedBill != null);

        public void AddInvoice()
        {
            var invoice = new Invoice
            {
                Id = Guid.NewGuid(),
                InvoiceNumber = GenerateInvoiceNumber(),
                InvoiceDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(30),
                Subtotal = 0,
                TaxAmount = 0,
                Total = 0,
                PaidAmount = 0,
                Balance = 0,
                Status = "Draft",
                Items = new List<InvoiceItem>()
            };
            Invoices.Add(invoice);
            SelectedInvoice = invoice;
        }

        public void AddBill()
        {
            var bill = new Bill
            {
                Id = Guid.NewGuid(),
                BillNumber = GenerateBillNumber(),
                BillDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(30),
                Subtotal = 0,
                TaxAmount = 0,
                Total = 0,
                PaidAmount = 0,
                Balance = 0,
                Status = "Draft",
                Items = new List<BillItem>()
            };
            Bills.Add(bill);
            SelectedBill = bill;
        }

        public void EditInvoice()
        {
            if (SelectedInvoice == null) return;

            // Calculate totals
            CalculateInvoiceTotals(SelectedInvoice);

            // Update the invoice in the collection
            int index = Invoices.IndexOf(SelectedInvoice);
            if (index >= 0)
            {
                var updatedInvoice = SelectedInvoice;
                updatedInvoice.UpdatedAt = DateTime.Now;
                Invoices[index] = updatedInvoice;
            }
        }

        public void EditBill()
        {
            if (SelectedBill == null) return;

            // Calculate totals
            CalculateBillTotals(SelectedBill);

            // Update the bill in the collection
            int index = Bills.IndexOf(SelectedBill);
            if (index >= 0)
            {
                var updatedBill = SelectedBill;
                updatedBill.UpdatedAt = DateTime.Now;
                Bills[index] = updatedBill;
            }
        }

        public void AddInvoiceItem()
        {
            if (SelectedInvoice == null) return;

            var item = new InvoiceItem
            {
                Id = Guid.NewGuid(),
                InvoiceId = SelectedInvoice.Id,
                Description = "New Item",
                Quantity = 1,
                UnitPrice = 0,
                Amount = 0,
                TaxRate = 0,
                ItemCode = ""
            };

            SelectedInvoice.Items.Add(item);
            CalculateInvoiceTotals(SelectedInvoice);
        }

        public void AddBillItem()
        {
            if (SelectedBill == null) return;

            var item = new BillItem
            {
                Id = Guid.NewGuid(),
                BillId = SelectedBill.Id,
                Description = "New Item",
                Quantity = 1,
                UnitPrice = 0,
                Amount = 0,
                TaxRate = 0,
                ItemCode = ""
            };

            SelectedBill.Items.Add(item);
            CalculateBillTotals(SelectedBill);
        }

        public void DeleteInvoiceItem()
        {
            if (SelectedInvoice == null || SelectedInvoiceItem == null) return;

            SelectedInvoice.Items.Remove(SelectedInvoiceItem);
            CalculateInvoiceTotals(SelectedInvoice);
            SelectedInvoiceItem = null;
        }

        public void DeleteBillItem()
        {
            if (SelectedBill == null || SelectedBillItem == null) return;

            SelectedBill.Items.Remove(SelectedBillItem);
            CalculateBillTotals(SelectedBill);
            SelectedBillItem = null;
        }

        public void DeleteInvoice()
        {
            if (SelectedInvoice != null) Invoices.Remove(SelectedInvoice);
        }

        public void DeleteBill()
        {
            if (SelectedBill != null) Bills.Remove(SelectedBill);
        }

        public async void GeneratePdf()
        {
            if (SelectedInvoice == null) return;

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                FileName = $"Invoice_{SelectedInvoice.InvoiceNumber}.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    await _pdfService.GenerateInvoicePdfAsync(SelectedInvoice, saveFileDialog.FileName);
                    System.Windows.MessageBox.Show($"Invoice PDF generated successfully: {saveFileDialog.FileName}",
                                                  "PDF Generation",
                                                  System.Windows.MessageBoxButton.OK,
                                                  System.Windows.MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error generating PDF: {ex.Message}",
                                                  "Error",
                                                  System.Windows.MessageBoxButton.OK,
                                                  System.Windows.MessageBoxImage.Error);
                }
            }
        }

        public void TrackPayment()
        {
            if (SelectedInvoice == null) return;

            // Open payment dialog (for now, just show a message)
            // In a real implementation, this would open a payment entry dialog
            System.Windows.MessageBox.Show($"Payment tracking for Invoice {SelectedInvoice.InvoiceNumber}\n" +
                                          $"Balance: ${SelectedInvoice.Balance:N2}\n" +
                                          $"Status: {SelectedInvoice.Status}",
                                          "Payment Tracking",
                                          System.Windows.MessageBoxButton.OK,
                                          System.Windows.MessageBoxImage.Information);
        }

        private void CalculateInvoiceTotals(Invoice invoice)
        {
            invoice.Subtotal = invoice.Items.Sum(item => item.Amount);
            invoice.TaxAmount = invoice.Items.Sum(item => item.Amount * item.TaxRate / 100);
            invoice.Total = invoice.Subtotal + invoice.TaxAmount;
            invoice.Balance = invoice.Total - invoice.PaidAmount;
            invoice.UpdatedAt = DateTime.Now;
        }

        private void CalculateBillTotals(Bill bill)
        {
            bill.Subtotal = bill.Items.Sum(item => item.Amount);
            bill.TaxAmount = bill.Items.Sum(item => item.Amount * item.TaxRate / 100);
            bill.Total = bill.Subtotal + bill.TaxAmount;
            bill.Balance = bill.Total - bill.PaidAmount;
            bill.UpdatedAt = DateTime.Now;
        }

        private string GenerateInvoiceNumber()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            return $"INV-{timestamp}";
        }

        private string GenerateBillNumber()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            return $"BILL-{timestamp}";
        }

        public async void GenerateBillPdf()
        {
            if (SelectedBill == null) return;

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                FileName = $"Bill_{SelectedBill.BillNumber}.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    await _pdfService.GenerateBillPdfAsync(SelectedBill, saveFileDialog.FileName);
                    System.Windows.MessageBox.Show($"Bill PDF generated successfully: {saveFileDialog.FileName}",
                                                  "PDF Generation",
                                                  System.Windows.MessageBoxButton.OK,
                                                  System.Windows.MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error generating PDF: {ex.Message}",
                                                  "Error",
                                                  System.Windows.MessageBoxButton.OK,
                                                  System.Windows.MessageBoxImage.Error);
                }
            }
        }
    }
}
