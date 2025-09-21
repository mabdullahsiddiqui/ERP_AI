using System.Windows;

namespace ERP_AI.Desktop.Services
{
    public interface INavigationService
    {
        void NavigateTo<T>() where T : Window, new();
        void NavigateTo<T>(object? parameter) where T : Window, new();
        void ShowDialog<T>() where T : Window, new();
        void ShowDialog<T>(object? parameter) where T : Window, new();
        void CloseCurrent();
        void GoBack();
        bool CanGoBack { get; }
    }
}

