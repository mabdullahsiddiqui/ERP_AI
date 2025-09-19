using ERP_AI.Core;
using System;
using System.Collections.ObjectModel;
using ERP_AI.Data;

namespace ERP_AI.Desktop.ViewModels
{
    public class InvoiceViewModel : BaseViewModel
    {
        public ObservableCollection<Invoice> Invoices { get; set; } = new();
        public ObservableCollection<Bill> Bills { get; set; } = new();
        public Invoice? SelectedInvoice { get; set; }
        public Bill? SelectedBill { get; set; }
        public RelayCommand AddInvoiceCommand => new(_ => AddInvoice());
        public RelayCommand AddBillCommand => new(_ => AddBill());
        public RelayCommand EditInvoiceCommand => new(_ => EditInvoice(), _ => SelectedInvoice != null);
        public RelayCommand EditBillCommand => new(_ => EditBill(), _ => SelectedBill != null);
        public RelayCommand DeleteInvoiceCommand => new(_ => DeleteInvoice(), _ => SelectedInvoice != null);
        public RelayCommand DeleteBillCommand => new(_ => DeleteBill(), _ => SelectedBill != null);
        public RelayCommand GeneratePdfCommand => new(_ => GeneratePdf(), _ => SelectedInvoice != null);
        public RelayCommand TrackPaymentCommand => new(_ => TrackPayment(), _ => SelectedInvoice != null);

        public void AddInvoice() { Invoices.Add(new Invoice { Total = 0 }); }
        public void AddBill() { Bills.Add(new Bill { Total = 0 }); }
        public void EditInvoice() { /* TODO: Implement edit logic */ }
        public void EditBill() { /* TODO: Implement edit logic */ }
        public void DeleteInvoice() { if (SelectedInvoice != null) Invoices.Remove(SelectedInvoice); }
        public void DeleteBill() { if (SelectedBill != null) Bills.Remove(SelectedBill); }
        public void GeneratePdf() { /* TODO: Implement PDF generation */ }
        public void TrackPayment() { /* TODO: Implement payment tracking */ }
    }
}
