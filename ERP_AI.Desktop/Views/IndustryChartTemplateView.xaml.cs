using System.Windows.Controls;
using ERP_AI.Desktop.ViewModels;

namespace ERP_AI.Desktop.Views
{
    public partial class IndustryChartTemplateView : UserControl
    {
        public IndustryChartTemplateView()
        {
            DataContext = new IndustryChartTemplateViewModel();
        }

    }
}
