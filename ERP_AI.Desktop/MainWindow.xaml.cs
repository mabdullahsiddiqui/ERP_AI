using System.Windows;
using System.Windows.Controls;

namespace ERP_AI.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Navigation.NavigationService _navigationService;

        public MainWindow()
        {
            InitializeComponent();
            _navigationService = new Navigation.NavigationService(MainContent);
            _navigationService.NavigateTo("Home");
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Header is string pageKey)
            {
                _navigationService.NavigateTo(pageKey);
            }
        }

        private void ThemeLight_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Set theme to Light using ModernWpfUI
        }

        private void ThemeDark_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Set theme to Dark using ModernWpfUI
        }
    }
}
