using System.Windows.Controls;
using ERP_AI.Desktop.ViewModels;

namespace ERP_AI.Desktop.Views
{
    public partial class TransactionView : UserControl
    {
        public TransactionView()
        {
            DataContext = new TransactionViewModel();
        }

    }
}
