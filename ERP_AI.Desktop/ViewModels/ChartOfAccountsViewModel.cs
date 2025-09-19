using System.Collections.ObjectModel;
using ERP_AI.Data;
using ERP_AI.Core;

namespace ERP_AI.Desktop.ViewModels
{
    public class ChartOfAccountsViewModel : BaseViewModel
    {
        public ObservableCollection<Account> Accounts { get; set; } = new();

        public IEnumerable<Account> RootAccounts => Accounts.Where(a => a.ParentId == null);

        public void BuildTree()
        {
            foreach (var account in Accounts)
            {
                account.Children.Clear();
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
        }
    }
}
