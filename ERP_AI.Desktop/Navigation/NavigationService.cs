using System;
using System.Collections.Generic;
using System.Windows.Controls;
using ERP_AI.Services;

namespace ERP_AI.Desktop.Navigation
{
    public class NavigationService : INavigationService
    {
        private readonly Dictionary<string, Func<UserControl>> _pageFactories = new();
        private readonly Stack<string> _history = new();
        private ContentControl _contentControl;

        public NavigationService(ContentControl contentControl)
        {
            _contentControl = contentControl;
            RegisterPages();
        }

        private void RegisterPages()
        {
            _pageFactories["Home"] = () => new Views.HomeView();
            _pageFactories["Accounts"] = () => new Views.AccountsView();
            _pageFactories["Sync"] = () => new Views.SyncView();
            _pageFactories["Settings"] = () => new Views.SettingsView();
        }

        public void NavigateTo(string pageKey, object? parameter = null)
        {
            if (_pageFactories.TryGetValue(pageKey, out var factory))
            {
                _contentControl.Content = factory();
                _history.Push(pageKey);
            }
        }

        public void GoBack()
        {
            if (_history.Count > 1)
            {
                _history.Pop();
                var previous = _history.Peek();
                NavigateTo(previous);
            }
        }

        public bool CanGoBack => _history.Count > 1;
        public void ClearHistory() => _history.Clear();
    }
}
