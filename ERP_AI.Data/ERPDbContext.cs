using Microsoft.EntityFrameworkCore;
using ERP_AI.Core;

namespace ERP_AI.Data
{
    public class ERPDbContext : DbContext
    {
        public ERPDbContext(DbContextOptions<ERPDbContext> options) : base(options) { }

        // DbSets for core entities
        public DbSet<Company> Companies { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Account> Accounts { get; set; } = null!;
        public DbSet<Transaction> Transactions { get; set; } = null!;
        public DbSet<TransactionDetail> TransactionDetails { get; set; } = null!;
        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<Vendor> Vendors { get; set; } = null!;
        public DbSet<Invoice> Invoices { get; set; } = null!;
        public DbSet<Bill> Bills { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<BankAccount> BankAccounts { get; set; } = null!;
        public DbSet<BankTransaction> BankTransactions { get; set; } = null!;
        // Sync-related entities
        public DbSet<SyncLog> SyncLogs { get; set; } = null!;
        public DbSet<ConflictResolution> ConflictResolutions { get; set; } = null!;
        public DbSet<CloudMapping> CloudMappings { get; set; } = null!;
        public DbSet<SyncQueue> SyncQueues { get; set; } = null!;
        public DbSet<SyncSettings> SyncSettings { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=erp_ai.db;Mode=ReadWriteCreate;Cache=Shared;Journal Mode=WAL");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Indexes, soft delete, audit, concurrency, seed data, etc. will be added here
        }
    }
}
