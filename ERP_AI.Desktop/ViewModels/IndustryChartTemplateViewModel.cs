using ERP_AI.Core;
using System.Collections.ObjectModel;
using ERP_AI.Data;

namespace ERP_AI.Desktop.ViewModels
{
    public class IndustryChartTemplateViewModel : BaseViewModel
    {
        public ObservableCollection<IndustryChartTemplate> Templates { get; set; } = new();
        public ObservableCollection<string> Industries { get; set; } = new();
        public IndustryChartTemplate? SelectedTemplate { get; set; }
        public string? SelectedIndustry { get; set; }

        public RelayCommand ImportTemplateCommand => new(_ => ImportTemplate());
        public RelayCommand ExportTemplateCommand => new(_ => ExportTemplate(), _ => SelectedTemplate != null);
        public RelayCommand CreateCustomTemplateCommand => new(_ => CreateCustomTemplate());

        public IndustryChartTemplateViewModel()
        {
            // Example industries and templates
            Industries.Add("General");
            Industries.Add("Retail");
            Industries.Add("Manufacturing");
            Industries.Add("Services");
            Industries.Add("Nonprofit");
            Templates.Add(new IndustryChartTemplate { Name = "Basic Chart", Industry = "General", Description = "Standard chart for small businesses." });
            Templates.Add(new IndustryChartTemplate { Name = "Retail Chart", Industry = "Retail", Description = "Chart for retail operations." });
        }

        private void ImportTemplate() { /* TODO: Implement import logic */ }
        private void ExportTemplate() { /* TODO: Implement export logic */ }
        private void CreateCustomTemplate() { /* TODO: Implement custom template creation */ }
    }
}
