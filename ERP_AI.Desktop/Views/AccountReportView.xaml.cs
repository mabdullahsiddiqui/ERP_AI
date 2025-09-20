using System.Windows.Controls;
using ERP_AI.Desktop.ViewModels;
using ERP_AI.Core;

namespace ERP_AI.Desktop.Views
{
    public partial class AccountReportView : UserControl
    {
        public AccountReportView()
        {
            DataContext = ViewModelLocator.Get<AccountReportViewModel>();
        }

    }
}
