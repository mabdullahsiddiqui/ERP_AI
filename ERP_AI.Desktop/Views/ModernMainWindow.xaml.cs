using System.Windows;

namespace ERP_AI.Desktop.Views
{
    /// <summary>
    /// Interaction logic for ModernMainWindow.xaml
    /// </summary>
    public partial class ModernMainWindow : Window
    {
        public ModernMainWindow()
        {
            InitializeComponent();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
