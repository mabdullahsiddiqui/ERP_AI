using System.Windows;
using System.Windows.Controls;

namespace ERP_AI.Desktop.Views
{
    /// <summary>
    /// Interaction logic for ReportsView.xaml
    /// </summary>
    public partial class ReportsView : UserControl
    {
        public ReportsView()
        {
            InitializeComponent();
        }

        private void ReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string reportName = button.Content.ToString();
                GenerateReport(reportName);
            }
        }

        private void GenerateReport(string reportName)
        {
            // Show report generation dialog or open report viewer
            MessageBox.Show($"Generating {reportName}...\n\nThis will open the report viewer with the selected report.", 
                          "Report Generation", MessageBoxButton.OK, MessageBoxImage.Information);
            
            // Here you would typically:
            // 1. Show a progress dialog
            // 2. Generate the report using the reporting service
            // 3. Open the report viewer with the generated report
            // 4. Handle any errors that occur during generation
        }

        private void QuickAction_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string action = button.Content.ToString();
                PerformQuickAction(action);
            }
        }

        private void PerformQuickAction(string action)
        {
            switch (action)
            {
                case "Generate All Financials":
                    MessageBox.Show("Generating all financial reports...", "Quick Action", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                case "Export to PDF":
                    MessageBox.Show("Exporting reports to PDF...", "Quick Action", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                case "Email Reports":
                    MessageBox.Show("Opening email dialog...", "Quick Action", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                case "Print Reports":
                    MessageBox.Show("Opening print dialog...", "Quick Action", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
            }
        }
    }
}
