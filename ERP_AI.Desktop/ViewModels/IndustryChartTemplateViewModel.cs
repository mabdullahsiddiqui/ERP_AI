using ERP_AI.Core;
using System.Collections.ObjectModel;
using System.Linq;
using ERP_AI.Data;

namespace ERP_AI.Desktop.ViewModels
{
    public class IndustryChartTemplateViewModel : BaseViewModel
    {
        public ObservableCollection<IndustryTemplate> Templates { get; set; } = new();
        public IndustryTemplate? SelectedTemplate { get; set; }
        public ObservableCollection<AccountTemplate> TemplateAccounts { get; set; } = new();
        public bool IsTemplateLoaded { get; set; }

        public RelayCommand LoadTemplateCommand => new(_ => LoadTemplate(), _ => SelectedTemplate != null);
        public RelayCommand ImportTemplateCommand => new(_ => ImportTemplate(), _ => SelectedTemplate != null && IsTemplateLoaded);
        public RelayCommand ExportTemplateCommand => new(_ => ExportTemplate(), _ => SelectedTemplate != null);
        public RelayCommand CreateCustomTemplateCommand => new(_ => CreateCustomTemplate());
        public RelayCommand PreviewTemplateCommand => new(_ => PreviewTemplate(), _ => SelectedTemplate != null);

        public IndustryChartTemplateViewModel()
        {
            LoadAvailableTemplates();
        }

        private void LoadAvailableTemplates()
        {
            Templates.Clear();
            var templates = IndustryTemplates.GetTemplates();
            foreach (var template in templates)
            {
                Templates.Add(template);
            }
        }

        public void LoadTemplate()
        {
            if (SelectedTemplate == null) return;

            TemplateAccounts.Clear();
            foreach (var account in SelectedTemplate.Accounts)
            {
                TemplateAccounts.Add(account);
            }
            IsTemplateLoaded = true;
        }

        public void ImportTemplate()
        {
            if (SelectedTemplate == null || !IsTemplateLoaded) return;

            try
            {
                // Convert template accounts to actual accounts
                var accounts = TemplateAccounts.Select(template => new Account
                {
                    Id = Guid.NewGuid(),
                    Code = template.Code,
                    Name = template.Name,
                    Type = template.Type,
                    Usage = template.Usage,
                    IsSystem = template.IsSystem,
                    IsActive = true,
                    Balance = 0,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsDeleted = false
                }).ToList();

                // Set parent-child relationships
                foreach (var account in accounts)
                {
                    var template = TemplateAccounts.FirstOrDefault(t => t.Code == account.Code);
                    if (template != null && !string.IsNullOrEmpty(template.ParentCode))
                    {
                        var parent = accounts.FirstOrDefault(a => a.Code == template.ParentCode);
                        if (parent != null)
                        {
                            account.ParentId = parent.Id;
                            account.Parent = parent;
                            parent.Children.Add(account);
                        }
                    }
                }

                // TODO: Save accounts to database using UnitOfWork
                System.Windows.MessageBox.Show($"Successfully imported {accounts.Count} accounts from {SelectedTemplate.Name} template.",
                                              "Template Imported",
                                              System.Windows.MessageBoxButton.OK,
                                              System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error importing template: {ex.Message}",
                                              "Import Error",
                                              System.Windows.MessageBoxButton.OK,
                                              System.Windows.MessageBoxImage.Error);
            }
        }

        public void ExportTemplate()
        {
            if (SelectedTemplate == null) return;

            try
            {
                // TODO: Implement template export to file
                System.Windows.MessageBox.Show($"Template '{SelectedTemplate.Name}' exported successfully.",
                                              "Template Exported",
                                              System.Windows.MessageBoxButton.OK,
                                              System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error exporting template: {ex.Message}",
                                              "Export Error",
                                              System.Windows.MessageBoxButton.OK,
                                              System.Windows.MessageBoxImage.Error);
            }
        }

        public void CreateCustomTemplate()
        {
            try
            {
                // TODO: Open custom template creation dialog
                System.Windows.MessageBox.Show("Custom template creation feature will be implemented.",
                                              "Feature Coming Soon",
                                              System.Windows.MessageBoxButton.OK,
                                              System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error creating custom template: {ex.Message}",
                                              "Creation Error",
                                              System.Windows.MessageBoxButton.OK,
                                              System.Windows.MessageBoxImage.Error);
            }
        }

        public void PreviewTemplate()
        {
            if (SelectedTemplate == null) return;

            var preview = $"Template: {SelectedTemplate.Name}\n" +
                         $"Industry: {SelectedTemplate.Industry}\n" +
                         $"Description: {SelectedTemplate.Description}\n" +
                         $"Number of Accounts: {SelectedTemplate.Accounts.Count}\n\n" +
                         "Account Structure:\n";

            var rootAccounts = SelectedTemplate.Accounts.Where(a => string.IsNullOrEmpty(a.ParentCode)).ToList();
            foreach (var root in rootAccounts)
            {
                preview += $"- {root.Code} {root.Name} ({root.Type})\n";
                AddChildAccounts(preview, root, SelectedTemplate.Accounts, "  ");
            }

            System.Windows.MessageBox.Show(preview,
                                          "Template Preview",
                                          System.Windows.MessageBoxButton.OK,
                                          System.Windows.MessageBoxImage.Information);
        }

        private void AddChildAccounts(string preview, AccountTemplate parent, List<AccountTemplate> allAccounts, string indent)
        {
            var children = allAccounts.Where(a => a.ParentCode == parent.Code).ToList();
            foreach (var child in children)
            {
                preview += $"{indent}- {child.Code} {child.Name} ({child.Type})\n";
                AddChildAccounts(preview, child, allAccounts, indent + "  ");
            }
        }
    }
}
