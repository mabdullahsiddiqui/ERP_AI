using System.Windows.Controls;
using ERP_AI.Desktop.ViewModels;

namespace ERP_AI.Desktop.Views
{
    public partial class AccountReportView : UserControl
    {
        public AccountReportView()
        {
            DataContext = new AccountReportViewModel();
        }

    }
}
