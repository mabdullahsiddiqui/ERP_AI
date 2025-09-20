using Microsoft.EntityFrameworkCore;
using ERP_AI.Core;
using System.Linq.Expressions;

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
        public DbSet<InvoiceItem> InvoiceItems { get; set; } = null!;
        public DbSet<Bill> Bills { get; set; } = null!;
        public DbSet<BillItem> BillItems { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<BankAccount> BankAccounts { get; set; } = null!;
        public DbSet<BankTransaction> BankTransactions { get; set; } = null!;
        public DbSet<Currency> Currencies { get; set; } = null!;
        public DbSet<ExchangeRate> ExchangeRates { get; set; } = null!;
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
            // Soft delete global filter
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(Core.BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(GetIsDeletedFilter(entityType.ClrType));
                }
            }

            // Audit trail: set UpdatedAt and UpdatedBy on save
            // Concurrency: RowVersion
            // Indexes for performance
            modelBuilder.Entity<Company>().HasIndex(e => e.Name);
            modelBuilder.Entity<Account>().HasIndex(e => e.Code);
            modelBuilder.Entity<Customer>().HasIndex(e => e.Name);
            modelBuilder.Entity<Vendor>().HasIndex(e => e.Name);

            // Full-text search setup (SQLite FTS5)
            // Example: modelBuilder.Entity<Account>().ToTable("Accounts", b => b.ExcludeFromMigrations());

            // Seed data for chart of accounts
            modelBuilder.Entity<Account>().HasData(new Account { Id = Guid.NewGuid(), Code = "1000", Name = "Cash" });
        }

        private static LambdaExpression GetIsDeletedFilter(Type entityType)
        {
            var param = Expression.Parameter(entityType, "e");
            var prop = Expression.Property(param, "IsDeleted");
            var condition = Expression.Equal(prop, Expression.Constant(false));
            return Expression.Lambda(condition, param);
        }
    }
}
