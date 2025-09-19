using ERP_AI.Core;
using System.Collections.ObjectModel;
using ERP_AI.Data;

namespace ERP_AI.Desktop.ViewModels
{
    public class ContactViewModel : BaseViewModel
    {
        public ObservableCollection<Customer> Customers { get; set; } = new();
        public ObservableCollection<Vendor> Vendors { get; set; } = new();
        public Customer? SelectedCustomer { get; set; }
        public Vendor? SelectedVendor { get; set; }
        public RelayCommand AddCustomerCommand => new(_ => AddCustomer());
        public RelayCommand AddVendorCommand => new(_ => AddVendor());
        public RelayCommand EditCustomerCommand => new(_ => EditCustomer(), _ => SelectedCustomer != null);
        public RelayCommand EditVendorCommand => new(_ => EditVendor(), _ => SelectedVendor != null);
        public RelayCommand DeleteCustomerCommand => new(_ => DeleteCustomer(), _ => SelectedCustomer != null);
        public RelayCommand DeleteVendorCommand => new(_ => DeleteVendor(), _ => SelectedVendor != null);

        public void AddCustomer() { Customers.Add(new Customer { Name = "New Customer" }); }
        public void AddVendor() { Vendors.Add(new Vendor { Name = "New Vendor" }); }
        public void EditCustomer() { /* TODO: Implement edit logic */ }
        public void EditVendor() { /* TODO: Implement edit logic */ }
        public void DeleteCustomer() { if (SelectedCustomer != null) Customers.Remove(SelectedCustomer); }
        public void DeleteVendor() { if (SelectedVendor != null) Vendors.Remove(SelectedVendor); }
    }
}
