using ERP_AI.Core;
using System;
using System.Collections.Generic;

namespace ERP_AI.Data
{
    public class IndustryTemplate
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public List<AccountTemplate> Accounts { get; set; } = new();
    }

    public class AccountTemplate
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public AccountType Type { get; set; }
        public string? ParentCode { get; set; }
        public string Usage { get; set; } = string.Empty;
        public bool IsSystem { get; set; } = false;
    }

    public static class IndustryTemplates
    {
        public static List<IndustryTemplate> GetTemplates()
        {
            return new List<IndustryTemplate>
            {
                GetRetailTemplate(),
                GetManufacturingTemplate(),
                GetServiceTemplate(),
                GetConstructionTemplate(),
                GetHealthcareTemplate(),
                GetNonProfitTemplate()
            };
        }

        public static IndustryTemplate GetRetailTemplate()
        {
            return new IndustryTemplate
            {
                Name = "Retail Business",
                Description = "Standard chart of accounts for retail businesses",
                Industry = "Retail",
                Accounts = new List<AccountTemplate>
                {
                    // Assets
                    new() { Code = "1000", Name = "Cash", Type = AccountType.Asset, Usage = "Cash" },
                    new() { Code = "1100", Name = "Petty Cash", Type = AccountType.Asset, Usage = "Cash" },
                    new() { Code = "1200", Name = "Checking Account", Type = AccountType.Asset, Usage = "Bank" },
                    new() { Code = "1300", Name = "Savings Account", Type = AccountType.Asset, Usage = "Bank" },
                    new() { Code = "1400", Name = "Accounts Receivable", Type = AccountType.Asset, Usage = "Receivable" },
                    new() { Code = "1500", Name = "Inventory", Type = AccountType.Asset, Usage = "Inventory" },
                    new() { Code = "1600", Name = "Prepaid Expenses", Type = AccountType.Asset, Usage = "Prepaid" },
                    new() { Code = "1700", Name = "Equipment", Type = AccountType.Asset, Usage = "Equipment" },
                    new() { Code = "1800", Name = "Accumulated Depreciation - Equipment", Type = AccountType.Asset, Usage = "AccumulatedDepreciation" },
                    new() { Code = "1900", Name = "Furniture & Fixtures", Type = AccountType.Asset, Usage = "Equipment" },
                    new() { Code = "1950", Name = "Accumulated Depreciation - Furniture", Type = AccountType.Asset, Usage = "AccumulatedDepreciation" },

                    // Liabilities
                    new() { Code = "2000", Name = "Accounts Payable", Type = AccountType.Liability, Usage = "Payable" },
                    new() { Code = "2100", Name = "Sales Tax Payable", Type = AccountType.Liability, Usage = "Tax" },
                    new() { Code = "2200", Name = "Payroll Tax Payable", Type = AccountType.Liability, Usage = "Tax" },
                    new() { Code = "2300", Name = "Accrued Expenses", Type = AccountType.Liability, Usage = "Accrued" },
                    new() { Code = "2400", Name = "Notes Payable", Type = AccountType.Liability, Usage = "Notes" },

                    // Equity
                    new() { Code = "3000", Name = "Owner's Equity", Type = AccountType.Equity, Usage = "Equity" },
                    new() { Code = "3100", Name = "Retained Earnings", Type = AccountType.Equity, Usage = "Equity" },

                    // Revenue
                    new() { Code = "4000", Name = "Sales Revenue", Type = AccountType.Revenue, Usage = "Sales" },
                    new() { Code = "4100", Name = "Service Revenue", Type = AccountType.Revenue, Usage = "Sales" },
                    new() { Code = "4200", Name = "Interest Income", Type = AccountType.Revenue, Usage = "Other" },

                    // Expenses
                    new() { Code = "5000", Name = "Cost of Goods Sold", Type = AccountType.Expense, Usage = "COGS" },
                    new() { Code = "5100", Name = "Advertising", Type = AccountType.Expense, Usage = "Marketing" },
                    new() { Code = "5200", Name = "Bank Charges", Type = AccountType.Expense, Usage = "Banking" },
                    new() { Code = "5300", Name = "Depreciation Expense", Type = AccountType.Expense, Usage = "Depreciation" },
                    new() { Code = "5400", Name = "Insurance", Type = AccountType.Expense, Usage = "Insurance" },
                    new() { Code = "5500", Name = "Office Supplies", Type = AccountType.Expense, Usage = "Supplies" },
                    new() { Code = "5600", Name = "Rent Expense", Type = AccountType.Expense, Usage = "Rent" },
                    new() { Code = "5700", Name = "Utilities", Type = AccountType.Expense, Usage = "Utilities" },
                    new() { Code = "5800", Name = "Wages & Salaries", Type = AccountType.Expense, Usage = "Payroll" },
                    new() { Code = "5900", Name = "Payroll Tax Expense", Type = AccountType.Expense, Usage = "Payroll" }
                }
            };
        }

        public static IndustryTemplate GetManufacturingTemplate()
        {
            return new IndustryTemplate
            {
                Name = "Manufacturing Business",
                Description = "Chart of accounts for manufacturing companies",
                Industry = "Manufacturing",
                Accounts = new List<AccountTemplate>
                {
                    // Assets
                    new() { Code = "1000", Name = "Cash", Type = AccountType.Asset, Usage = "Cash" },
                    new() { Code = "1100", Name = "Accounts Receivable", Type = AccountType.Asset, Usage = "Receivable" },
                    new() { Code = "1200", Name = "Raw Materials Inventory", Type = AccountType.Asset, Usage = "Inventory" },
                    new() { Code = "1300", Name = "Work in Process Inventory", Type = AccountType.Asset, Usage = "Inventory" },
                    new() { Code = "1400", Name = "Finished Goods Inventory", Type = AccountType.Asset, Usage = "Inventory" },
                    new() { Code = "1500", Name = "Manufacturing Equipment", Type = AccountType.Asset, Usage = "Equipment" },
                    new() { Code = "1600", Name = "Accumulated Depreciation - Equipment", Type = AccountType.Asset, Usage = "AccumulatedDepreciation" },
                    new() { Code = "1700", Name = "Factory Building", Type = AccountType.Asset, Usage = "Building" },
                    new() { Code = "1800", Name = "Accumulated Depreciation - Building", Type = AccountType.Asset, Usage = "AccumulatedDepreciation" },

                    // Liabilities
                    new() { Code = "2000", Name = "Accounts Payable", Type = AccountType.Liability, Usage = "Payable" },
                    new() { Code = "2100", Name = "Accrued Wages", Type = AccountType.Liability, Usage = "Accrued" },
                    new() { Code = "2200", Name = "Notes Payable", Type = AccountType.Liability, Usage = "Notes" },

                    // Equity
                    new() { Code = "3000", Name = "Owner's Equity", Type = AccountType.Equity, Usage = "Equity" },
                    new() { Code = "3100", Name = "Retained Earnings", Type = AccountType.Equity, Usage = "Equity" },

                    // Revenue
                    new() { Code = "4000", Name = "Sales Revenue", Type = AccountType.Revenue, Usage = "Sales" },

                    // Expenses
                    new() { Code = "5000", Name = "Cost of Goods Sold", Type = AccountType.Expense, Usage = "COGS" },
                    new() { Code = "5100", Name = "Direct Materials", Type = AccountType.Expense, Usage = "COGS" },
                    new() { Code = "5200", Name = "Direct Labor", Type = AccountType.Expense, Usage = "COGS" },
                    new() { Code = "5300", Name = "Manufacturing Overhead", Type = AccountType.Expense, Usage = "COGS" },
                    new() { Code = "5400", Name = "Factory Rent", Type = AccountType.Expense, Usage = "Rent" },
                    new() { Code = "5500", Name = "Factory Utilities", Type = AccountType.Expense, Usage = "Utilities" },
                    new() { Code = "5600", Name = "Depreciation Expense", Type = AccountType.Expense, Usage = "Depreciation" }
                }
            };
        }

        public static IndustryTemplate GetServiceTemplate()
        {
            return new IndustryTemplate
            {
                Name = "Service Business",
                Description = "Chart of accounts for service-based businesses",
                Industry = "Service",
                Accounts = new List<AccountTemplate>
                {
                    // Assets
                    new() { Code = "1000", Name = "Cash", Type = AccountType.Asset, Usage = "Cash" },
                    new() { Code = "1100", Name = "Accounts Receivable", Type = AccountType.Asset, Usage = "Receivable" },
                    new() { Code = "1200", Name = "Office Equipment", Type = AccountType.Asset, Usage = "Equipment" },
                    new() { Code = "1300", Name = "Accumulated Depreciation - Equipment", Type = AccountType.Asset, Usage = "AccumulatedDepreciation" },
                    new() { Code = "1400", Name = "Prepaid Expenses", Type = AccountType.Asset, Usage = "Prepaid" },

                    // Liabilities
                    new() { Code = "2000", Name = "Accounts Payable", Type = AccountType.Liability, Usage = "Payable" },
                    new() { Code = "2100", Name = "Accrued Expenses", Type = AccountType.Liability, Usage = "Accrued" },

                    // Equity
                    new() { Code = "3000", Name = "Owner's Equity", Type = AccountType.Equity, Usage = "Equity" },
                    new() { Code = "3100", Name = "Retained Earnings", Type = AccountType.Equity, Usage = "Equity" },

                    // Revenue
                    new() { Code = "4000", Name = "Service Revenue", Type = AccountType.Revenue, Usage = "Sales" },
                    new() { Code = "4100", Name = "Consulting Revenue", Type = AccountType.Revenue, Usage = "Sales" },

                    // Expenses
                    new() { Code = "5000", Name = "Professional Fees", Type = AccountType.Expense, Usage = "Professional" },
                    new() { Code = "5100", Name = "Office Rent", Type = AccountType.Expense, Usage = "Rent" },
                    new() { Code = "5200", Name = "Office Supplies", Type = AccountType.Expense, Usage = "Supplies" },
                    new() { Code = "5300", Name = "Telephone", Type = AccountType.Expense, Usage = "Utilities" },
                    new() { Code = "5400", Name = "Internet", Type = AccountType.Expense, Usage = "Utilities" },
                    new() { Code = "5500", Name = "Insurance", Type = AccountType.Expense, Usage = "Insurance" },
                    new() { Code = "5600", Name = "Depreciation Expense", Type = AccountType.Expense, Usage = "Depreciation" }
                }
            };
        }

        public static IndustryTemplate GetConstructionTemplate()
        {
            return new IndustryTemplate
            {
                Name = "Construction Business",
                Description = "Chart of accounts for construction companies",
                Industry = "Construction",
                Accounts = new List<AccountTemplate>
                {
                    // Assets
                    new() { Code = "1000", Name = "Cash", Type = AccountType.Asset, Usage = "Cash" },
                    new() { Code = "1100", Name = "Accounts Receivable", Type = AccountType.Asset, Usage = "Receivable" },
                    new() { Code = "1200", Name = "Construction Equipment", Type = AccountType.Asset, Usage = "Equipment" },
                    new() { Code = "1300", Name = "Accumulated Depreciation - Equipment", Type = AccountType.Asset, Usage = "AccumulatedDepreciation" },
                    new() { Code = "1400", Name = "Construction Materials", Type = AccountType.Asset, Usage = "Inventory" },
                    new() { Code = "1500", Name = "Work in Progress", Type = AccountType.Asset, Usage = "Inventory" },

                    // Liabilities
                    new() { Code = "2000", Name = "Accounts Payable", Type = AccountType.Liability, Usage = "Payable" },
                    new() { Code = "2100", Name = "Retainage Payable", Type = AccountType.Liability, Usage = "Payable" },
                    new() { Code = "2200", Name = "Notes Payable", Type = AccountType.Liability, Usage = "Notes" },

                    // Equity
                    new() { Code = "3000", Name = "Owner's Equity", Type = AccountType.Equity, Usage = "Equity" },
                    new() { Code = "3100", Name = "Retained Earnings", Type = AccountType.Equity, Usage = "Equity" },

                    // Revenue
                    new() { Code = "4000", Name = "Construction Revenue", Type = AccountType.Revenue, Usage = "Sales" },

                    // Expenses
                    new() { Code = "5000", Name = "Cost of Construction", Type = AccountType.Expense, Usage = "COGS" },
                    new() { Code = "5100", Name = "Materials", Type = AccountType.Expense, Usage = "COGS" },
                    new() { Code = "5200", Name = "Labor", Type = AccountType.Expense, Usage = "COGS" },
                    new() { Code = "5300", Name = "Subcontractor Costs", Type = AccountType.Expense, Usage = "COGS" },
                    new() { Code = "5400", Name = "Equipment Rental", Type = AccountType.Expense, Usage = "Rental" },
                    new() { Code = "5500", Name = "Fuel & Oil", Type = AccountType.Expense, Usage = "Fuel" },
                    new() { Code = "5600", Name = "Depreciation Expense", Type = AccountType.Expense, Usage = "Depreciation" }
                }
            };
        }

        public static IndustryTemplate GetHealthcareTemplate()
        {
            return new IndustryTemplate
            {
                Name = "Healthcare Practice",
                Description = "Chart of accounts for healthcare practices",
                Industry = "Healthcare",
                Accounts = new List<AccountTemplate>
                {
                    // Assets
                    new() { Code = "1000", Name = "Cash", Type = AccountType.Asset, Usage = "Cash" },
                    new() { Code = "1100", Name = "Accounts Receivable", Type = AccountType.Asset, Usage = "Receivable" },
                    new() { Code = "1200", Name = "Medical Equipment", Type = AccountType.Asset, Usage = "Equipment" },
                    new() { Code = "1300", Name = "Accumulated Depreciation - Equipment", Type = AccountType.Asset, Usage = "AccumulatedDepreciation" },
                    new() { Code = "1400", Name = "Medical Supplies", Type = AccountType.Asset, Usage = "Inventory" },
                    new() { Code = "1500", Name = "Prepaid Insurance", Type = AccountType.Asset, Usage = "Prepaid" },

                    // Liabilities
                    new() { Code = "2000", Name = "Accounts Payable", Type = AccountType.Liability, Usage = "Payable" },
                    new() { Code = "2100", Name = "Accrued Payroll", Type = AccountType.Liability, Usage = "Accrued" },
                    new() { Code = "2200", Name = "Notes Payable", Type = AccountType.Liability, Usage = "Notes" },

                    // Equity
                    new() { Code = "3000", Name = "Owner's Equity", Type = AccountType.Equity, Usage = "Equity" },
                    new() { Code = "3100", Name = "Retained Earnings", Type = AccountType.Equity, Usage = "Equity" },

                    // Revenue
                    new() { Code = "4000", Name = "Patient Revenue", Type = AccountType.Revenue, Usage = "Sales" },
                    new() { Code = "4100", Name = "Insurance Revenue", Type = AccountType.Revenue, Usage = "Sales" },

                    // Expenses
                    new() { Code = "5000", Name = "Medical Supplies Expense", Type = AccountType.Expense, Usage = "Supplies" },
                    new() { Code = "5100", Name = "Staff Salaries", Type = AccountType.Expense, Usage = "Payroll" },
                    new() { Code = "5200", Name = "Rent Expense", Type = AccountType.Expense, Usage = "Rent" },
                    new() { Code = "5300", Name = "Utilities", Type = AccountType.Expense, Usage = "Utilities" },
                    new() { Code = "5400", Name = "Insurance Expense", Type = AccountType.Expense, Usage = "Insurance" },
                    new() { Code = "5500", Name = "Professional Fees", Type = AccountType.Expense, Usage = "Professional" },
                    new() { Code = "5600", Name = "Depreciation Expense", Type = AccountType.Expense, Usage = "Depreciation" }
                }
            };
        }

        public static IndustryTemplate GetNonProfitTemplate()
        {
            return new IndustryTemplate
            {
                Name = "Non-Profit Organization",
                Description = "Chart of accounts for non-profit organizations",
                Industry = "NonProfit",
                Accounts = new List<AccountTemplate>
                {
                    // Assets
                    new() { Code = "1000", Name = "Cash", Type = AccountType.Asset, Usage = "Cash" },
                    new() { Code = "1100", Name = "Accounts Receivable", Type = AccountType.Asset, Usage = "Receivable" },
                    new() { Code = "1200", Name = "Pledges Receivable", Type = AccountType.Asset, Usage = "Receivable" },
                    new() { Code = "1300", Name = "Prepaid Expenses", Type = AccountType.Asset, Usage = "Prepaid" },
                    new() { Code = "1400", Name = "Office Equipment", Type = AccountType.Asset, Usage = "Equipment" },
                    new() { Code = "1500", Name = "Accumulated Depreciation - Equipment", Type = AccountType.Asset, Usage = "AccumulatedDepreciation" },

                    // Liabilities
                    new() { Code = "2000", Name = "Accounts Payable", Type = AccountType.Liability, Usage = "Payable" },
                    new() { Code = "2100", Name = "Accrued Expenses", Type = AccountType.Liability, Usage = "Accrued" },
                    new() { Code = "2200", Name = "Deferred Revenue", Type = AccountType.Liability, Usage = "Deferred" },

                    // Net Assets
                    new() { Code = "3000", Name = "Unrestricted Net Assets", Type = AccountType.Equity, Usage = "Equity" },
                    new() { Code = "3100", Name = "Temporarily Restricted Net Assets", Type = AccountType.Equity, Usage = "Equity" },
                    new() { Code = "3200", Name = "Permanently Restricted Net Assets", Type = AccountType.Equity, Usage = "Equity" },

                    // Revenue
                    new() { Code = "4000", Name = "Contributions", Type = AccountType.Revenue, Usage = "Contributions" },
                    new() { Code = "4100", Name = "Grants", Type = AccountType.Revenue, Usage = "Grants" },
                    new() { Code = "4200", Name = "Program Revenue", Type = AccountType.Revenue, Usage = "Program" },
                    new() { Code = "4300", Name = "Investment Income", Type = AccountType.Revenue, Usage = "Investment" },

                    // Expenses
                    new() { Code = "5000", Name = "Program Expenses", Type = AccountType.Expense, Usage = "Program" },
                    new() { Code = "5100", Name = "Administrative Expenses", Type = AccountType.Expense, Usage = "Administrative" },
                    new() { Code = "5200", Name = "Fundraising Expenses", Type = AccountType.Expense, Usage = "Fundraising" },
                    new() { Code = "5300", Name = "Salaries & Wages", Type = AccountType.Expense, Usage = "Payroll" },
                    new() { Code = "5400", Name = "Office Rent", Type = AccountType.Expense, Usage = "Rent" },
                    new() { Code = "5500", Name = "Office Supplies", Type = AccountType.Expense, Usage = "Supplies" },
                    new() { Code = "5600", Name = "Utilities", Type = AccountType.Expense, Usage = "Utilities" }
                }
            };
        }
    }
}
