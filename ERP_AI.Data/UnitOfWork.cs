using System;
using System.Threading.Tasks;
using ERP_AI.Core;

namespace ERP_AI.Data
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Company> Companies { get; }
        IRepository<User> Users { get; }
        IRepository<Account> Accounts { get; }
        IRepository<Transaction> Transactions { get; }
        IRepository<TransactionDetail> TransactionDetails { get; }
        IRepository<Customer> Customers { get; }
        IRepository<Vendor> Vendors { get; }
        IRepository<Invoice> Invoices { get; }
        IRepository<InvoiceItem> InvoiceItems { get; }
        IRepository<Bill> Bills { get; }
        IRepository<BillItem> BillItems { get; }
        IRepository<Payment> Payments { get; }
        IRepository<BankAccount> BankAccounts { get; }
        IRepository<BankTransaction> BankTransactions { get; }
        IRepository<SyncLog> SyncLogs { get; }
        IRepository<ConflictResolution> ConflictResolutions { get; }
        IRepository<CloudMapping> CloudMappings { get; }
        IRepository<SyncQueue> SyncQueues { get; }
        IRepository<SyncSettings> SyncSettings { get; }
        Task<int> SaveChangesAsync();
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly ERPDbContext _context;
        public IRepository<Company> Companies { get; }
        public IRepository<User> Users { get; }
        public IRepository<Account> Accounts { get; }
        public IRepository<Transaction> Transactions { get; }
        public IRepository<TransactionDetail> TransactionDetails { get; }
        public IRepository<Customer> Customers { get; }
        public IRepository<Vendor> Vendors { get; }
        public IRepository<Invoice> Invoices { get; }
        public IRepository<InvoiceItem> InvoiceItems { get; }
        public IRepository<Bill> Bills { get; }
        public IRepository<BillItem> BillItems { get; }
        public IRepository<Payment> Payments { get; }
        public IRepository<BankAccount> BankAccounts { get; }
        public IRepository<BankTransaction> BankTransactions { get; }
        public IRepository<SyncLog> SyncLogs { get; }
        public IRepository<ConflictResolution> ConflictResolutions { get; }
        public IRepository<CloudMapping> CloudMappings { get; }
        public IRepository<SyncQueue> SyncQueues { get; }
        public IRepository<SyncSettings> SyncSettings { get; }

        public UnitOfWork(ERPDbContext context)
        {
            _context = context;
            Companies = new Repository<Company>(context);
            Users = new Repository<User>(context);
            Accounts = new Repository<Account>(context);
            Transactions = new Repository<Transaction>(context);
            TransactionDetails = new Repository<TransactionDetail>(context);
            Customers = new Repository<Customer>(context);
            Vendors = new Repository<Vendor>(context);
            Invoices = new Repository<Invoice>(context);
            InvoiceItems = new Repository<InvoiceItem>(context);
            Bills = new Repository<Bill>(context);
            BillItems = new Repository<BillItem>(context);
            Payments = new Repository<Payment>(context);
            BankAccounts = new Repository<BankAccount>(context);
            BankTransactions = new Repository<BankTransaction>(context);
            SyncLogs = new Repository<SyncLog>(context);
            ConflictResolutions = new Repository<ConflictResolution>(context);
            CloudMappings = new Repository<CloudMapping>(context);
            SyncQueues = new Repository<SyncQueue>(context);
            SyncSettings = new Repository<SyncSettings>(context);
        }

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
        public void Dispose() => _context.Dispose();
    }
}
