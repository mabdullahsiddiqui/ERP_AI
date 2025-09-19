using System.Collections;
using System.Windows.Controls;
using System.Windows;
using ERP_AI.Desktop.ViewModels;

namespace ERP_AI.Desktop.Views
{
    public partial class AccountsListView : UserControl
    {
        public AccountsListView()
        {
            InitializeComponent();
            AccountsList.SelectionChanged += AccountsList_SelectionChanged;
        }

    private void AccountsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is AccountListViewModel vm)
            {
                vm.SelectedAccounts.Clear();
                foreach (var item in ((ListView)sender).SelectedItems)
                {
                    if (item is ERP_AI.Data.Account acc)
                        vm.SelectedAccounts.Add(acc);
                }
            }
        }
    }
}
