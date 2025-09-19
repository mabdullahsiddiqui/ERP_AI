using System.Collections.ObjectModel;
using ERP_AI.Data;
using ERP_AI.Core;

namespace ERP_AI.Desktop.ViewModels
{
    public class AccountReportViewModel : BaseViewModel
    {
        public ObservableCollection<Account> Accounts { get; set; } = new();
        public ObservableCollection<string> ReportTypes { get; set; } = new();
        public string? SelectedReportType { get; set; }
        public ObservableCollection<object> ReportData { get; set; } = new();

        public RelayCommand GenerateReportCommand => new(_ => GenerateReport());

        public AccountReportViewModel()
        {
            ReportTypes.Add("Chart of Accounts");
            ReportTypes.Add("Account Activity");
            ReportTypes.Add("Balance Report");
            ReportTypes.Add("Trial Balance");
            ReportTypes.Add("Aging Analysis");
        }

        private void GenerateReport()
        {
            ReportData.Clear();
            switch (SelectedReportType)
            {
                case "Chart of Accounts":
                    foreach (var acc in Accounts)
                        ReportData.Add(new { acc.Code, acc.Name, acc.Type, acc.Balance, acc.IsActive });
                    break;
                case "Account Activity":
                    // Placeholder: Add logic to fetch account activity (e.g., transactions)
                    foreach (var acc in Accounts)
                        ReportData.Add(new { acc.Code, acc.Name, Activity = "N/A" });
                    break;
                case "Balance Report":
                    foreach (var acc in Accounts)
                        ReportData.Add(new { acc.Code, acc.Name, acc.Balance });
                    break;
                case "Trial Balance":
                    // Placeholder: Add logic for trial balance (debits/credits)
                    foreach (var acc in Accounts)
                        ReportData.Add(new { acc.Code, acc.Name, acc.Balance, Debit = 0m, Credit = 0m });
                    break;
                case "Aging Analysis":
                    // Placeholder: Add logic for aging analysis (e.g., receivables/payables)
                    foreach (var acc in Accounts)
                        ReportData.Add(new { acc.Code, acc.Name, acc.Balance, Aging = "N/A" });
                    break;
                default:
                    break;
            }
        }
    }
}
