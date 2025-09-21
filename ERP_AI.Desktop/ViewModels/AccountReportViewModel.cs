using System.Collections.ObjectModel;
using System;
using System.Linq;
using ERP_AI.Data;
using ERP_AI.Core;
using ERP_AI.Services;

namespace ERP_AI.Desktop.ViewModels
{
    public class AccountReportViewModel : BaseViewModel
    {
        private readonly IReportingService _reportingService;

        public AccountReportViewModel(IReportingService reportingService)
        {
            _reportingService = reportingService;
        }

        public ObservableCollection<Account> Accounts { get; set; } = new();
        public ObservableCollection<ReportTemplate> AvailableReports { get; set; } = new();
        public ReportTemplate? SelectedReport { get; set; }
        public ObservableCollection<object> ReportData { get; set; } = new();
        public DateTime FromDate { get; set; } = DateTime.Now.AddMonths(-1);
        public DateTime ToDate { get; set; } = DateTime.Now;
        public DateTime AsOfDate { get; set; } = DateTime.Now;
        public bool IsGenerating { get; set; }

        public RelayCommand LoadReportsCommand => new(_ => LoadReports());
        public RelayCommand GenerateReportCommand => new(_ => GenerateReport(), _ => SelectedReport != null && !IsGenerating);
        public RelayCommand ExportToPdfCommand => new(_ => ExportToPdf(), _ => ReportData.Any() && !IsGenerating);
        public RelayCommand ExportToExcelCommand => new(_ => ExportToExcel(), _ => ReportData.Any() && !IsGenerating);

        public async void LoadReports()
        {
            try
            {
                AvailableReports.Clear();
                var reports = await _reportingService.GetAvailableReportsAsync();
                foreach (var report in reports)
                {
                    AvailableReports.Add(report);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading reports: {ex.Message}",
                                              "Error",
                                              System.Windows.MessageBoxButton.OK,
                                              System.Windows.MessageBoxImage.Error);
            }
        }

        public async void GenerateReport()
        {
            if (SelectedReport == null) return;

            IsGenerating = true;
            try
            {
                ReportData.Clear();
                object reportData = null;

                switch (SelectedReport.Name)
                {
                    case "Trial Balance":
                        reportData = await _reportingService.GenerateTrialBalanceAsync(AsOfDate);
                        break;
                    case "Profit & Loss":
                        reportData = await _reportingService.GenerateProfitLossAsync(FromDate, ToDate);
                        break;
                    case "Balance Sheet":
                        reportData = await _reportingService.GenerateBalanceSheetAsync(AsOfDate);
                        break;
                    case "Customer Aging":
                        reportData = await _reportingService.GenerateAgingReportAsync(AsOfDate, "Customer");
                        break;
                    case "Vendor Aging":
                        reportData = await _reportingService.GenerateAgingReportAsync(AsOfDate, "Vendor");
                        break;
                    case "Cash Flow":
                        reportData = await _reportingService.GenerateCashFlowAsync(FromDate, ToDate);
                        break;
                    default:
                        System.Windows.MessageBox.Show("Report type not implemented yet.",
                                                      "Not Implemented",
                                                      System.Windows.MessageBoxButton.OK,
                                                      System.Windows.MessageBoxImage.Information);
                        return;
                }

                if (reportData != null)
                {
                    ReportData.Add(reportData);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error generating report: {ex.Message}",
                                              "Error",
                                              System.Windows.MessageBoxButton.OK,
                                              System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsGenerating = false;
            }
        }

        public async void ExportToPdf()
        {
            if (!ReportData.Any() || SelectedReport == null) return;

            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "PDF Files (*.pdf)|*.pdf",
                    FileName = $"{SelectedReport.Name}_{DateTime.Now:yyyyMMdd}.pdf"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    await _reportingService.ExportReportToPdfAsync(ReportData.First(), SelectedReport.Name, saveFileDialog.FileName);
                    System.Windows.MessageBox.Show($"Report exported to PDF: {saveFileDialog.FileName}",
                                                  "Export Complete",
                                                  System.Windows.MessageBoxButton.OK,
                                                  System.Windows.MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error exporting to PDF: {ex.Message}",
                                              "Export Error",
                                              System.Windows.MessageBoxButton.OK,
                                              System.Windows.MessageBoxImage.Error);
            }
        }

        public async void ExportToExcel()
        {
            if (!ReportData.Any() || SelectedReport == null) return;

            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    FileName = $"{SelectedReport.Name}_{DateTime.Now:yyyyMMdd}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    await _reportingService.ExportReportToExcelAsync(ReportData.First(), SelectedReport.Name, saveFileDialog.FileName);
                    System.Windows.MessageBox.Show($"Report exported to Excel: {saveFileDialog.FileName}",
                                                  "Export Complete",
                                                  System.Windows.MessageBoxButton.OK,
                                                  System.Windows.MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error exporting to Excel: {ex.Message}",
                                              "Export Error",
                                              System.Windows.MessageBoxButton.OK,
                                              System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
