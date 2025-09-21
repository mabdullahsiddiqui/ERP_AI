using System.Windows.Controls;
using ERP_AI.Desktop.ViewModels;
using ERP_AI.Core;

namespace ERP_AI.Desktop.Views
{
    public partial class InvoiceView : UserControl
    {
        public InvoiceView()
        {
            DataContext = ViewModelLocator.Get<InvoiceViewModel>();
        }

    }
}
