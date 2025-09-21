using System.Windows;

namespace ERP_AI.Desktop.Services
{
    public class NavigationService : INavigationService
    {
        private readonly Stack<Window> _navigationStack = new();
        private Window? _currentWindow;

        public bool CanGoBack => _navigationStack.Count > 0;

        public void NavigateTo<T>() where T : Window, new()
        {
            NavigateTo<T>(null);
        }

        public void NavigateTo<T>(object? parameter) where T : Window, new()
        {
            var newWindow = new T();
            
            if (_currentWindow != null)
            {
                _navigationStack.Push(_currentWindow);
                _currentWindow.Hide();
            }

            _currentWindow = newWindow;
            newWindow.Show();
        }

        public void ShowDialog<T>() where T : Window, new()
        {
            ShowDialog<T>(null);
        }

        public void ShowDialog<T>(object? parameter) where T : Window, new()
        {
            var dialog = new T();
            dialog.ShowDialog();
        }

        public void CloseCurrent()
        {
            if (_currentWindow != null)
            {
                _currentWindow.Close();
                _currentWindow = null;
            }

            if (CanGoBack)
            {
                _currentWindow = _navigationStack.Pop();
                _currentWindow.Show();
            }
        }

        public void GoBack()
        {
            if (CanGoBack)
            {
                if (_currentWindow != null)
                {
                    _currentWindow.Hide();
                }

                _currentWindow = _navigationStack.Pop();
                _currentWindow.Show();
            }
        }
    }
}

