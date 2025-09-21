using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Debug;
using Microsoft.Extensions.Http;
using ERP_AI.Core;
using ERP_AI.Data;
using ERP_AI.Services;
using ERP_AI.Desktop.Services;
using ERP_AI.Desktop.Models;
using ModernWpf;

namespace ERP_AI.Desktop;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Initialize ModernWpf theme
        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
        
        // Configure services
        var services = new ServiceCollection();
        ConfigureServices(services);
        
        // Initialize ViewModelLocator
        ViewModelLocator.Init(services);
        
        // Show login window for authentication
        var serviceProvider = services.BuildServiceProvider();
        var loginView = new Views.LoginView(
            serviceProvider.GetRequiredService<IAuthenticationService>(),
            serviceProvider.GetRequiredService<ERP_AI.Desktop.Services.INavigationService>()
        );
        loginView.Show();
    }
    
    private void ConfigureServices(ServiceCollection services)
    {
        // Add Configuration
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
        services.AddSingleton<IConfiguration>(configuration);

        // Add Logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
        });

        // Add HttpClient
        services.AddHttpClient<IAuthenticationService, AuthenticationService>();

        // Register DbContext
        services.AddDbContext<ERPDbContext>();
        
        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // Register Services
        services.AddScoped<IPdfService, PdfService>();
        services.AddScoped<IReportingService, ReportingService>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddScoped<IAuthenticationService, MockAuthenticationService>();
        services.AddScoped<IRoleService, MockRoleService>();
        services.AddScoped<IPasswordPolicyService, MockPasswordPolicyService>();
        services.AddScoped<IAuditService, MockAuditService>();
        services.AddScoped<ERP_AI.Desktop.Services.INavigationService, NavigationService>();
        
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

