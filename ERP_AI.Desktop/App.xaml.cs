using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using ERP_AI.Core;
using ERP_AI.Data;
using ERP_AI.Services;

namespace ERP_AI.Desktop;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Configure services
        var services = new ServiceCollection();
        ConfigureServices(services);
        
        // Initialize ViewModelLocator
        ViewModelLocator.Init(services);
    }
    
    private void ConfigureServices(ServiceCollection services)
    {
        // Register DbContext
        services.AddDbContext<ERPDbContext>();
        
        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // Register Services
        services.AddScoped<IPdfService, PdfService>();
        services.AddScoped<IReportingService, ReportingService>();
        
        // Register ViewModels
        services.AddTransient<ViewModels.AccountListViewModel>();
        services.AddTransient<ViewModels.ChartOfAccountsViewModel>();
        services.AddTransient<ViewModels.AccountReportViewModel>();
        services.AddTransient<ViewModels.AccountEditViewModel>();
        services.AddTransient<ViewModels.ContactViewModel>();
        services.AddTransient<ViewModels.InvoiceViewModel>();
        services.AddTransient<ViewModels.TransactionViewModel>();
        services.AddTransient<ViewModels.IndustryChartTemplateViewModel>();
    }
}

