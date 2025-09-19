
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Data;
using CsvHelper;
using ExcelDataReader;
using ERP_AI.Data;
using ERP_AI.Core;


namespace ERP_AI.Desktop.ViewModels
{
    public class AccountListViewModel : BaseViewModel
    {
        private readonly OfflineChangeTracker _changeTracker = new();
        public ObservableCollection<Account> Accounts { get; set; } = new();
        public string SearchText { get; set; } = string.Empty;
        public ObservableCollection<Account> FilteredAccounts =>
            new(Accounts.Where(a => (a.Name.Contains(SearchText) || a.Code.Contains(SearchText)) && a.IsActive));

        public ObservableCollection<Account> SelectedAccounts { get; set; } = new();

        public RelayCommand DeleteSelectedCommand => new(_ => DeleteSelected(), _ => SelectedAccounts.Any());
        public RelayCommand ActivateSelectedCommand => new(_ => ActivateSelected(), _ => SelectedAccounts.Any());
        public RelayCommand DeactivateSelectedCommand => new(_ => DeactivateSelected(), _ => SelectedAccounts.Any());
        public RelayCommand MoveSelectedCommand => new(_ => MoveSelected(), _ => SelectedAccounts.Any());
        public RelayCommand ImportAccountsCommand => new(_ => ImportAccounts());

        public void AddAccount(Account account)
        {
            Accounts.Add(account);
            UpdateParentChild(account);
            _changeTracker.TrackChange("Account", account.Id, ChangeType.Add, Newtonsoft.Json.JsonConvert.SerializeObject(account));
        }

        private void UpdateParentChild(Account account)
        {
            if (account.ParentId.HasValue)
            {
                var parent = Accounts.FirstOrDefault(a => a.Id == account.ParentId.Value);
                if (parent != null)
                {
                    account.Parent = parent;
                    parent.Children.Add(account);
                }
            }
        }

        private void DeleteSelected()
        {
            foreach (var acc in SelectedAccounts.ToList())
            {
                Accounts.Remove(acc);
                _changeTracker.TrackChange("Account", acc.Id, ChangeType.Delete, Newtonsoft.Json.JsonConvert.SerializeObject(acc));
            }
            SelectedAccounts.Clear();
        }

        private void ActivateSelected()
        {
            foreach (var acc in SelectedAccounts)
            {
                acc.IsActive = true;
                _changeTracker.TrackChange("Account", acc.Id, ChangeType.Update, Newtonsoft.Json.JsonConvert.SerializeObject(acc));
            }
        }

        private void DeactivateSelected()
        {
            foreach (var acc in SelectedAccounts)
            {
                acc.IsActive = false;
                _changeTracker.TrackChange("Account", acc.Id, ChangeType.Update, Newtonsoft.Json.JsonConvert.SerializeObject(acc));
            }
        }

        private void MoveSelected()
        {
            // Placeholder: implement move logic (e.g., set new ParentId)
        }

        private void ImportAccounts()
        {
            // Show OpenFileDialog for CSV/Excel selection
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                Title = "Import Accounts from CSV/Excel"
            };
            if (dialog.ShowDialog() == true)
            {
                var filePath = dialog.FileName;
                if (filePath.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    ImportAccountsFromCsv(filePath);
                }
                else if (filePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    ImportAccountsFromExcel(filePath);
                }
            }
        }

        private void ImportAccountsFromCsv(string filePath)
        {
            try
            {
                using var reader = new StreamReader(filePath);
                using var csv = new CsvHelper.CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture);
                var records = csv.GetRecords<AccountCsvModel>().ToList();
                foreach (var record in records)
                {
                    // Map CSV model to Account entity
                    var account = new Account
                    {
                        Code = record.Code,
                        Name = record.Name,
                        Type = Enum.TryParse<AccountType>(record.Type, out var type) ? type : AccountType.Asset,
                        IsActive = record.IsActive,
                        Usage = record.Usage
                    };
                    Accounts.Add(account);
                }
            }
            catch (Exception)
            {
                // TODO: Show error message to user
            }
        }

        private void ImportAccountsFromExcel(string filePath)
        {
            try
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
                using var reader = ExcelReaderFactory.CreateReader(stream);
                var result = reader.AsDataSet();
                var table = result.Tables[0];
                for (int i = 1; i < table.Rows.Count; i++) // Assuming first row is header
                {
                    var row = table.Rows[i];
                    var account = new Account
                    {
                        Code = row[0]?.ToString() ?? string.Empty,
                        Name = row[1]?.ToString() ?? string.Empty,
                        Type = Enum.TryParse<AccountType>(row[2]?.ToString(), out var type) ? type : AccountType.Asset,
                        IsActive = bool.TryParse(row[3]?.ToString(), out var active) ? active : true,
                        Usage = row[4]?.ToString() ?? string.Empty
                    };
                    Accounts.Add(account);
                }
            }
            catch (Exception)
            {
                // TODO: Show error message to user
            }
        }

        private class AccountCsvModel
        {
            public string Code { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Type { get; set; } = "Asset";
            public bool IsActive { get; set; } = true;
            public string Usage { get; set; } = string.Empty;
        }
    }
// Removed extra closing brace
}
