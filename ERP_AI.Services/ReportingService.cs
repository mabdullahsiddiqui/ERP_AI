using ERP_AI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ERP_AI.Services
{
    public class ReportingService : IReportingService
    {
        private readonly ERPDbContext _context;

        public ReportingService(ERPDbContext context)
        {
            _context = context;
        }

        public async Task<TrialBalanceReport> GenerateTrialBalanceAsync(DateTime asOfDate)
        {
            var report = new TrialBalanceReport
            {
                AsOfDate = asOfDate
            };

            var accounts = await _context.Accounts
                .Where(a => !a.IsDeleted && a.IsActive)
                .OrderBy(a => a.Code)
                .ToListAsync();

            foreach (var account in accounts)
            {
                var balance = await CalculateAccountBalanceAsync(account.Id, asOfDate);
                var item = new TrialBalanceItem
                {
                    AccountCode = account.Code,
                    AccountName = account.Name,
                    AccountType = account.Type
                };

                // Determine if account has debit or credit balance based on account type
                if (account.Type == AccountType.Asset || account.Type == AccountType.Expense)
                {
                    item.DebitBalance = Math.Abs(balance);
                    item.CreditBalance = 0;
                }
                else
                {
                    item.DebitBalance = 0;
                    item.CreditBalance = Math.Abs(balance);
                }

                report.Items.Add(item);
            }

            report.TotalDebits = report.Items.Sum(i => i.DebitBalance);
            report.TotalCredits = report.Items.Sum(i => i.CreditBalance);

            return report;
        }

        public async Task<ProfitLossReport> GenerateProfitLossAsync(DateTime fromDate, DateTime toDate)
        {
            var report = new ProfitLossReport
            {
                FromDate = fromDate,
                ToDate = toDate
            };

            // Get revenue accounts
            var revenueAccounts = await _context.Accounts
                .Where(a => !a.IsDeleted && a.IsActive && a.Type == AccountType.Revenue)
                .OrderBy(a => a.Code)
                .ToListAsync();

            foreach (var account in revenueAccounts)
            {
                var amount = await CalculateAccountActivityAsync(account.Id, fromDate, toDate);
                if (amount != 0)
                {
                    report.RevenueItems.Add(new ProfitLossItem
                    {
                        AccountCode = account.Code,
                        AccountName = account.Name,
                        Amount = amount
                    });
                }
            }

            // Get expense accounts
            var expenseAccounts = await _context.Accounts
                .Where(a => !a.IsDeleted && a.IsActive && a.Type == AccountType.Expense)
                .OrderBy(a => a.Code)
                .ToListAsync();

            foreach (var account in expenseAccounts)
            {
                var amount = await CalculateAccountActivityAsync(account.Id, fromDate, toDate);
                if (amount != 0)
                {
                    report.ExpenseItems.Add(new ProfitLossItem
                    {
                        AccountCode = account.Code,
                        AccountName = account.Name,
                        Amount = amount
                    });
                }
            }

            report.TotalRevenue = report.RevenueItems.Sum(i => i.Amount);
            report.TotalExpenses = report.ExpenseItems.Sum(i => i.Amount);

            // Calculate percentages
            foreach (var item in report.RevenueItems)
            {
                item.Percentage = report.TotalRevenue != 0 ? (item.Amount / report.TotalRevenue) * 100 : 0;
            }

            foreach (var item in report.ExpenseItems)
            {
                item.Percentage = report.TotalRevenue != 0 ? (item.Amount / report.TotalRevenue) * 100 : 0;
            }

            return report;
        }

        public async Task<BalanceSheetReport> GenerateBalanceSheetAsync(DateTime asOfDate)
        {
            var report = new BalanceSheetReport
            {
                AsOfDate = asOfDate
            };

            // Get asset accounts
            var assetAccounts = await _context.Accounts
                .Where(a => !a.IsDeleted && a.IsActive && a.Type == AccountType.Asset)
                .OrderBy(a => a.Code)
                .ToListAsync();

            foreach (var account in assetAccounts)
            {
                var balance = await CalculateAccountBalanceAsync(account.Id, asOfDate);
                if (balance != 0)
                {
                    report.Assets.Add(new BalanceSheetItem
                    {
                        AccountCode = account.Code,
                        AccountName = account.Name,
                        Amount = balance
                    });
                }
            }

            // Get liability accounts
            var liabilityAccounts = await _context.Accounts
                .Where(a => !a.IsDeleted && a.IsActive && a.Type == AccountType.Liability)
                .OrderBy(a => a.Code)
                .ToListAsync();

            foreach (var account in liabilityAccounts)
            {
                var balance = await CalculateAccountBalanceAsync(account.Id, asOfDate);
                if (balance != 0)
                {
                    report.Liabilities.Add(new BalanceSheetItem
                    {
                        AccountCode = account.Code,
                        AccountName = account.Name,
                        Amount = balance
                    });
                }
            }

            // Get equity accounts
            var equityAccounts = await _context.Accounts
                .Where(a => !a.IsDeleted && a.IsActive && a.Type == AccountType.Equity)
                .OrderBy(a => a.Code)
                .ToListAsync();

            foreach (var account in equityAccounts)
            {
                var balance = await CalculateAccountBalanceAsync(account.Id, asOfDate);
                if (balance != 0)
                {
                    report.Equity.Add(new BalanceSheetItem
                    {
                        AccountCode = account.Code,
                        AccountName = account.Name,
                        Amount = balance
                    });
                }
            }

            report.TotalAssets = report.Assets.Sum(a => a.Amount);
            report.TotalLiabilities = report.Liabilities.Sum(l => l.Amount);
            report.TotalEquity = report.Equity.Sum(e => e.Amount);

            // Calculate percentages
            foreach (var item in report.Assets)
            {
                item.Percentage = report.TotalAssets != 0 ? (item.Amount / report.TotalAssets) * 100 : 0;
            }

            foreach (var item in report.Liabilities)
            {
                item.Percentage = report.TotalLiabilities != 0 ? (item.Amount / report.TotalLiabilities) * 100 : 0;
            }

            foreach (var item in report.Equity)
            {
                item.Percentage = report.TotalEquity != 0 ? (item.Amount / report.TotalEquity) * 100 : 0;
            }

            return report;
        }

        public async Task<AgingReport> GenerateAgingReportAsync(DateTime asOfDate, string entityType)
        {
            var report = new AgingReport
            {
                AsOfDate = asOfDate,
                EntityType = entityType
            };

            if (entityType == "Customer")
            {
                var customers = await _context.Customers
                    .Where(c => !c.IsDeleted && c.IsActive)
                    .ToListAsync();

                foreach (var customer in customers)
                {
                    var aging = await CalculateCustomerAgingAsync(customer.Id, asOfDate);
                    if (aging.Total > 0)
                    {
                        report.Items.Add(aging);
                    }
                }
            }
            else if (entityType == "Vendor")
            {
                var vendors = await _context.Vendors
                    .Where(v => !v.IsDeleted && v.IsActive)
                    .ToListAsync();

                foreach (var vendor in vendors)
                {
                    var aging = await CalculateVendorAgingAsync(vendor.Id, asOfDate);
                    if (aging.Total > 0)
                    {
                        report.Items.Add(aging);
                    }
                }
            }

            report.TotalCurrent = report.Items.Sum(i => i.Current);
            report.Total30Days = report.Items.Sum(i => i.Days30);
            report.Total60Days = report.Items.Sum(i => i.Days60);
            report.Total90Days = report.Items.Sum(i => i.Days90);
            report.TotalOver90Days = report.Items.Sum(i => i.Over90Days);
            report.GrandTotal = report.Items.Sum(i => i.Total);

            return report;
        }

        public async Task<CashFlowReport> GenerateCashFlowAsync(DateTime fromDate, DateTime toDate)
        {
            var report = new CashFlowReport
            {
                FromDate = fromDate,
                ToDate = toDate
            };

            // Operating cash flow - cash accounts activity
            var cashAccounts = await _context.Accounts
                .Where(a => !a.IsDeleted && a.IsActive && a.Usage == "Cash")
                .ToListAsync();

            foreach (var account in cashAccounts)
            {
                var activity = await CalculateAccountActivityAsync(account.Id, fromDate, toDate);
                report.OperatingItems.Add(new CashFlowItem
                {
                    Description = $"Cash Activity - {account.Name}",
                    Amount = activity
                });
            }

            report.OperatingCashFlow = report.OperatingItems.Sum(i => i.Amount);

            // Investing cash flow - equipment and asset purchases
            var equipmentAccounts = await _context.Accounts
                .Where(a => !a.IsDeleted && a.IsActive && a.Usage == "Equipment")
                .ToListAsync();

            foreach (var account in equipmentAccounts)
            {
                var activity = await CalculateAccountActivityAsync(account.Id, fromDate, toDate);
                report.InvestingItems.Add(new CashFlowItem
                {
                    Description = $"Equipment - {account.Name}",
                    Amount = activity
                });
            }

            report.InvestingCashFlow = report.InvestingItems.Sum(i => i.Amount);

            // Financing cash flow - loans and equity
            var loanAccounts = await _context.Accounts
                .Where(a => !a.IsDeleted && a.IsActive && a.Usage == "Notes")
                .ToListAsync();

            foreach (var account in loanAccounts)
            {
                var activity = await CalculateAccountActivityAsync(account.Id, fromDate, toDate);
                report.FinancingItems.Add(new CashFlowItem
                {
                    Description = $"Financing - {account.Name}",
                    Amount = activity
                });
            }

            report.FinancingCashFlow = report.FinancingItems.Sum(i => i.Amount);

            return report;
        }

        public async Task<AccountActivityReport> GenerateAccountActivityAsync(Guid accountId, DateTime fromDate, DateTime toDate)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null)
                throw new ArgumentException("Account not found");

            var report = new AccountActivityReport
            {
                AccountId = accountId,
                AccountName = account.Name,
                FromDate = fromDate,
                ToDate = toDate
            };

            // Get beginning balance
            report.BeginningBalance = await CalculateAccountBalanceAsync(accountId, fromDate.AddDays(-1));

            // Get transactions
            var transactions = await _context.TransactionDetails
                .Include(td => td.Transaction)
                .Where(td => td.AccountId == accountId && 
                            td.Transaction.Date >= fromDate && 
                            td.Transaction.Date <= toDate &&
                            !td.IsDeleted)
                .OrderBy(td => td.Transaction.Date)
                .ToListAsync();

            decimal runningBalance = report.BeginningBalance;

            foreach (var transaction in transactions)
            {
                var item = new AccountActivityItem
                {
                    Date = transaction.Transaction.Date,
                    Reference = transaction.Transaction.Reference,
                    Description = transaction.Description,
                    Debit = transaction.IsDebit ? transaction.Amount : 0,
                    Credit = !transaction.IsDebit ? transaction.Amount : 0
                };

                runningBalance += item.Debit - item.Credit;
                item.Balance = runningBalance;

                report.Transactions.Add(item);
            }

            report.EndingBalance = runningBalance;

            return report;
        }

        public async Task<List<ReportTemplate>> GetAvailableReportsAsync()
        {
            return await Task.FromResult(new List<ReportTemplate>
            {
                new() { Name = "Trial Balance", Description = "Shows all account balances", Category = "Financial", RequiresAsOfDate = true },
                new() { Name = "Profit & Loss", Description = "Income statement for a period", Category = "Financial", RequiresDateRange = true },
                new() { Name = "Balance Sheet", Description = "Financial position as of a date", Category = "Financial", RequiresAsOfDate = true },
                new() { Name = "Customer Aging", Description = "Outstanding customer balances", Category = "Aging", RequiresAsOfDate = true, Parameters = new() { "Customer" } },
                new() { Name = "Vendor Aging", Description = "Outstanding vendor balances", Category = "Aging", RequiresAsOfDate = true, Parameters = new() { "Vendor" } },
                new() { Name = "Cash Flow", Description = "Cash flow statement", Category = "Financial", RequiresDateRange = true },
                new() { Name = "Account Activity", Description = "Detailed account transactions", Category = "Detail", RequiresDateRange = true, Parameters = new() { "Account" } }
            });
        }

        public async Task<string> ExportReportToPdfAsync(object reportData, string reportType, string outputPath)
        {
            // TODO: Implement PDF export using PdfService
            await Task.Delay(100); // Placeholder
            return outputPath;
        }

        public async Task<string> ExportReportToExcelAsync(object reportData, string reportType, string outputPath)
        {
            // TODO: Implement Excel export
            await Task.Delay(100); // Placeholder
            return outputPath;
        }

        private async Task<decimal> CalculateAccountBalanceAsync(Guid accountId, DateTime asOfDate)
        {
            var debits = await _context.TransactionDetails
                .Where(td => td.AccountId == accountId && 
                            td.IsDebit && 
                            td.Transaction.Date <= asOfDate &&
                            !td.IsDeleted)
                .SumAsync(td => td.Amount);

            var credits = await _context.TransactionDetails
                .Where(td => td.AccountId == accountId && 
                            !td.IsDebit && 
                            td.Transaction.Date <= asOfDate &&
                            !td.IsDeleted)
                .SumAsync(td => td.Amount);

            return debits - credits;
        }

        private async Task<decimal> CalculateAccountActivityAsync(Guid accountId, DateTime fromDate, DateTime toDate)
        {
            var debits = await _context.TransactionDetails
                .Where(td => td.AccountId == accountId && 
                            td.IsDebit && 
                            td.Transaction.Date >= fromDate && 
                            td.Transaction.Date <= toDate &&
                            !td.IsDeleted)
                .SumAsync(td => td.Amount);

            var credits = await _context.TransactionDetails
                .Where(td => td.AccountId == accountId && 
                            !td.IsDebit && 
                            td.Transaction.Date >= fromDate && 
                            td.Transaction.Date <= toDate &&
                            !td.IsDeleted)
                .SumAsync(td => td.Amount);

            return debits - credits;
        }

        private async Task<AgingItem> CalculateCustomerAgingAsync(Guid customerId, DateTime asOfDate)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null) return new AgingItem();

            var invoices = await _context.Invoices
                .Where(i => i.CustomerId == customerId && 
                           i.DueDate <= asOfDate && 
                           i.Balance > 0 &&
                           !i.IsDeleted)
                .ToListAsync();

            var aging = new AgingItem
            {
                EntityName = customer.Name
            };

            foreach (var invoice in invoices)
            {
                var daysPastDue = (asOfDate - invoice.DueDate).Days;

                if (daysPastDue <= 0)
                    aging.Current += invoice.Balance;
                else if (daysPastDue <= 30)
                    aging.Days30 += invoice.Balance;
                else if (daysPastDue <= 60)
                    aging.Days60 += invoice.Balance;
                else if (daysPastDue <= 90)
                    aging.Days90 += invoice.Balance;
                else
                    aging.Over90Days += invoice.Balance;
            }

            aging.Total = aging.Current + aging.Days30 + aging.Days60 + aging.Days90 + aging.Over90Days;

            return aging;
        }

        private async Task<AgingItem> CalculateVendorAgingAsync(Guid vendorId, DateTime asOfDate)
        {
            var vendor = await _context.Vendors.FindAsync(vendorId);
            if (vendor == null) return new AgingItem();

            var bills = await _context.Bills
                .Where(b => b.VendorId == vendorId && 
                           b.DueDate <= asOfDate && 
                           b.Balance > 0 &&
                           !b.IsDeleted)
                .ToListAsync();

            var aging = new AgingItem
            {
                EntityName = vendor.Name
            };

            foreach (var bill in bills)
            {
                var daysPastDue = (asOfDate - bill.DueDate).Days;

                if (daysPastDue <= 0)
                    aging.Current += bill.Balance;
                else if (daysPastDue <= 30)
                    aging.Days30 += bill.Balance;
                else if (daysPastDue <= 60)
                    aging.Days60 += bill.Balance;
                else if (daysPastDue <= 90)
                    aging.Days90 += bill.Balance;
                else
                    aging.Over90Days += bill.Balance;
            }

            aging.Total = aging.Current + aging.Days30 + aging.Days60 + aging.Days90 + aging.Over90Days;

            return aging;
        }
    }
}
