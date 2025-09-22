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
        // Phase 3 sync entities
        public DbSet<SyncStatusTracker> SyncStatusTrackers { get; set; } = null!;
        public DbSet<ChangeTracker> ChangeTrackers { get; set; } = null!;
        public DbSet<TombstoneRecord> TombstoneRecords { get; set; } = null!;
        
        // Bank Reconciliation
        public DbSet<BankStatement> BankStatements { get; set; } = null!;
        public DbSet<BankStatementItem> BankStatementItems { get; set; } = null!;
        public DbSet<BankReconciliation> BankReconciliations { get; set; } = null!;
        public DbSet<ReconciliationItem> ReconciliationItems { get; set; } = null!;
        public DbSet<OutstandingItem> OutstandingItems { get; set; } = null!;
        public DbSet<ImportTemplate> ImportTemplates { get; set; } = null!;
        public DbSet<ImportRule> ImportRules { get; set; } = null!;
        public DbSet<MatchingRule> MatchingRules { get; set; } = null!;
        public DbSet<ReconciliationReport> ReconciliationReports { get; set; } = null!;
        public DbSet<ReconciliationAudit> ReconciliationAudits { get; set; } = null!;
        
        // Cash Flow Management
        public DbSet<CashFlowForecast> CashFlowForecasts { get; set; } = null!;
        public DbSet<CashFlowProjection> CashFlowProjections { get; set; } = null!;
        public DbSet<CashFlowScenario> CashFlowScenarios { get; set; } = null!;
        public DbSet<CashFlowDependency> CashFlowDependencies { get; set; } = null!;
        public DbSet<PaymentSchedule> PaymentSchedules { get; set; } = null!;
        public DbSet<PaymentScheduleItem> PaymentScheduleItems { get; set; } = null!;
        public DbSet<WorkingCapitalAnalysis> WorkingCapitalAnalyses { get; set; } = null!;
        public DbSet<WorkingCapitalTrend> WorkingCapitalTrends { get; set; } = null!;
        public DbSet<LiquidityPlan> LiquidityPlans { get; set; } = null!;
        public DbSet<LiquidityAction> LiquidityActions { get; set; } = null!;
        public DbSet<LiquidityScenario> LiquidityScenarios { get; set; } = null!;
        public DbSet<CashPosition> CashPositions { get; set; } = null!;
        public DbSet<CashPositionDetail> CashPositionDetails { get; set; } = null!;
        public DbSet<PaymentTimingAnalysis> PaymentTimingAnalyses { get; set; } = null!;
        public DbSet<PaymentTimingPattern> PaymentTimingPatterns { get; set; } = null!;
        public DbSet<PaymentTimingRecommendation> PaymentTimingRecommendations { get; set; } = null!;
        
        // Budgeting & Forecasting
        public DbSet<Budget> Budgets { get; set; } = null!;
        public DbSet<BudgetItem> BudgetItems { get; set; } = null!;
        public DbSet<BudgetItemDetail> BudgetItemDetails { get; set; } = null!;
        public DbSet<BudgetVariance> BudgetVariances { get; set; } = null!;
        public DbSet<BudgetScenario> BudgetScenarios { get; set; } = null!;
        public DbSet<BudgetScenarioItem> BudgetScenarioItems { get; set; } = null!;
        public DbSet<DepartmentBudget> DepartmentBudgets { get; set; } = null!;
        public DbSet<DepartmentBudgetItem> DepartmentBudgetItems { get; set; } = null!;
        public DbSet<ProjectBudget> ProjectBudgets { get; set; } = null!;
        public DbSet<ProjectBudgetItem> ProjectBudgetItems { get; set; } = null!;
        public DbSet<RollingForecast> RollingForecasts { get; set; } = null!;
        public DbSet<RollingForecastItem> RollingForecastItems { get; set; } = null!;
        public DbSet<BudgetTemplate> BudgetTemplates { get; set; } = null!;
        public DbSet<BudgetTemplateItem> BudgetTemplateItems { get; set; } = null!;
        public DbSet<BudgetAlert> BudgetAlerts { get; set; } = null!;
        public DbSet<BudgetApproval> BudgetApprovals { get; set; } = null!;
        public DbSet<BudgetPerformance> BudgetPerformances { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=erp_ai.db;Cache=Shared");
            }
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

            // Configure CashFlowDependency relationships to avoid circular reference
            modelBuilder.Entity<CashFlowDependency>()
                .HasOne(d => d.Projection)
                .WithMany(p => p.Dependencies)
                .HasForeignKey(d => d.ProjectionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CashFlowDependency>()
                .HasOne(d => d.DependentProjection)
                .WithMany()
                .HasForeignKey(d => d.DependentProjectionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure CashFlowScenario-Projection relationship
            modelBuilder.Entity<CashFlowProjection>()
                .HasOne(p => p.Scenario)
                .WithMany(s => s.Projections)
                .HasForeignKey(p => p.ScenarioId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure LiquidityAction-Scenario relationship
            modelBuilder.Entity<LiquidityAction>()
                .HasOne(a => a.Scenario)
                .WithMany(s => s.Actions)
                .HasForeignKey(a => a.ScenarioId)
                .OnDelete(DeleteBehavior.SetNull);

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
