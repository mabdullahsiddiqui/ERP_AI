using System.Windows.Controls;
using ERP_AI.Desktop.ViewModels;

namespace ERP_AI.Desktop.Views
{
    public partial class InvoiceView : UserControl
    {
        public InvoiceView()
        {
            DataContext = new InvoiceViewModel();
        }

    }
}
