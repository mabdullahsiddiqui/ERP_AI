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
        public void EditCustomer()
        {
            if (SelectedCustomer == null) return;

            // Create a copy of the customer for editing
            var editedCustomer = new Customer
            {
                Id = SelectedCustomer.Id,
                Name = SelectedCustomer.Name,
                Email = SelectedCustomer.Email,
                Phone = SelectedCustomer.Phone,
                Address = SelectedCustomer.Address,
                City = SelectedCustomer.City,
                State = SelectedCustomer.State,
                ZipCode = SelectedCustomer.ZipCode,
                Country = SelectedCustomer.Country,
                CreditLimit = SelectedCustomer.CreditLimit,
                CurrentBalance = SelectedCustomer.CurrentBalance,
                PaymentTerms = SelectedCustomer.PaymentTerms,
                IsActive = SelectedCustomer.IsActive,
                Notes = SelectedCustomer.Notes,
                CreatedAt = SelectedCustomer.CreatedAt,
                UpdatedAt = DateTime.Now,
                IsDeleted = SelectedCustomer.IsDeleted
            };

            // Update the customer in the collection
            int index = Customers.IndexOf(SelectedCustomer);
            if (index >= 0)
            {
                Customers[index] = editedCustomer;
                SelectedCustomer = editedCustomer;
            }
        }

        public void EditVendor()
        {
            if (SelectedVendor == null) return;

            // Create a copy of the vendor for editing
            var editedVendor = new Vendor
            {
                Id = SelectedVendor.Id,
                Name = SelectedVendor.Name,
                Email = SelectedVendor.Email,
                Phone = SelectedVendor.Phone,
                Address = SelectedVendor.Address,
                City = SelectedVendor.City,
                State = SelectedVendor.State,
                ZipCode = SelectedVendor.ZipCode,
                Country = SelectedVendor.Country,
                TaxId = SelectedVendor.TaxId,
                PaymentTerms = SelectedVendor.PaymentTerms,
                IsActive = SelectedVendor.IsActive,
                Notes = SelectedVendor.Notes,
                CreatedAt = SelectedVendor.CreatedAt,
                UpdatedAt = DateTime.Now,
                IsDeleted = SelectedVendor.IsDeleted
            };

            // Update the vendor in the collection
            int index = Vendors.IndexOf(SelectedVendor);
            if (index >= 0)
            {
                Vendors[index] = editedVendor;
                SelectedVendor = editedVendor;
            }
        }
        public void DeleteCustomer() { if (SelectedCustomer != null) Customers.Remove(SelectedCustomer); }
        public void DeleteVendor() { if (SelectedVendor != null) Vendors.Remove(SelectedVendor); }
    }
}
