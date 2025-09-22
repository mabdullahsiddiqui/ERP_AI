using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ERP_AI.Desktop.Models;
using ERP_AI.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ERP_AI.Desktop.Views
{
    public partial class LoginView : Window, INotifyPropertyChanged
    {
        private readonly IAuthenticationService _authService;
        private readonly INavigationService _navigationService;

        public LoginView(IAuthenticationService authService, INavigationService navigationService)
        {
            _authService = authService;
            _navigationService = navigationService;
            
            InitializeComponent();
            DataContext = this;

            // Subscribe to authentication events
            _authService.AuthenticationStateChanged += OnAuthenticationStateChanged;
            _authService.UserLoggedIn += OnUserLoggedIn;

            // Initialize commands
            LoginCommand = new AsyncRelayCommand(LoginAsync, CanLogin);
            NavigateToRegisterCommand = new RelayCommand(NavigateToRegister);
            ForgotPasswordCommand = new RelayCommand(ForgotPassword);

            // Check connection status
            _ = Task.Run(CheckConnectionAsync);

            // Set focus to email field and pre-populate password
            Loaded += (s, e) => 
            {
                EmailTextBox.Focus();
                PasswordBox.Password = "password123";
                LoginCommand.NotifyCanExecuteChanged();
            };
            
            // Add password change handler
            PasswordBox.PasswordChanged += (s, e) => LoginCommand.NotifyCanExecuteChanged();
        }

        #region Properties

        private string _email = "admin@erpai.com";
        public string Email
        {
            get => _email;
            set
            {
                if (SetProperty(ref _email, value))
                {
                    ValidateEmail();
                    LoginCommand.NotifyCanExecuteChanged();
                }
            }
        }


        private bool _rememberMe = false;
        public bool RememberMe
        {
            get => _rememberMe;
            set => SetProperty(ref _rememberMe, value);
        }

        private string _emailError = string.Empty;
        public string EmailError
        {
            get => _emailError;
            set => SetProperty(ref _emailError, value);
        }

        private string _passwordError = string.Empty;
        public string PasswordError
        {
            get => _passwordError;
            set => SetProperty(ref _passwordError, value);
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        private bool _hasEmailError = false;
        public bool HasEmailError
        {
            get => _hasEmailError;
            set => SetProperty(ref _hasEmailError, value);
        }

        private bool _hasPasswordError = false;
        public bool HasPasswordError
        {
            get => _hasPasswordError;
            set => SetProperty(ref _hasPasswordError, value);
        }

        private bool _hasError = false;
        public bool HasError
        {
            get => _hasError;
            set => SetProperty(ref _hasError, value);
        }

        private bool _isLoading = false;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private bool _isOnline = true;
        public bool IsOnline
        {
            get => _isOnline;
            set => SetProperty(ref _isOnline, value);
        }

        private string _connectionStatus = "Checking connection...";
        public string ConnectionStatus
        {
            get => _connectionStatus;
            set => SetProperty(ref _connectionStatus, value);
        }

        #endregion

        #region Commands

        public IAsyncRelayCommand LoginCommand { get; }
        public IRelayCommand NavigateToRegisterCommand { get; }
        public IRelayCommand ForgotPasswordCommand { get; }

        #endregion

        #region Command Implementations

        private async Task LoginAsync()
        {
            try
            {
                IsLoading = true;
                ClearErrors();

                var request = new LoginRequest
                {
                    Email = Email.Trim(),
                    Password = PasswordBox.Password,
                    RememberMe = RememberMe
                };

                var response = await _authService.LoginAsync(request);

                if (response.Success)
                {
                    // Success - the event handlers will handle showing the main window
                    // The UserLoggedIn or AuthenticationStateChanged event will be fired
                }
                else
                {
                    ErrorMessage = response.ErrorMessage ?? "Login failed. Please check your credentials.";
                    HasError = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred during login. Please try again.";
                HasError = true;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(Email) && 
                   !string.IsNullOrWhiteSpace(PasswordBox?.Password) && 
                   !IsLoading;
        }

        private void NavigateToRegister()
        {
            var registerView = new RegisterView(_authService, _navigationService);
            registerView.Show();
            Close();
        }

        private void ForgotPassword()
        {
            MessageBox.Show("Password reset functionality will be implemented in a future update.", 
                          "Forgot Password", 
                          MessageBoxButton.OK, 
                          MessageBoxImage.Information);
        }

        private void OnUserLoggedIn(object? sender, UserInfo userInfo)
        {
            // Handle user logged in event
            Dispatcher.Invoke(() =>
            {
                ShowMainWindow();
                Close();
            });
        }

        private void OnAuthenticationStateChanged(object? sender, bool isAuthenticated)
        {
            // Handle authentication state change
            Dispatcher.Invoke(() =>
            {
                if (isAuthenticated)
                {
                    ShowMainWindow();
                    Close();
                }
            });
        }

        private void ShowMainWindow()
        {
            try
            {
                // Create and show the main window
                var mainWindow = new Views.Phase5DashboardView();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening main window: {ex.Message}", "Error", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Validation

        private void ValidateEmail()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                EmailError = "Email is required";
                HasEmailError = true;
            }
            else if (!IsValidEmail(Email))
            {
                EmailError = "Please enter a valid email address";
                HasEmailError = true;
            }
            else
            {
                EmailError = string.Empty;
                HasEmailError = false;
            }
        }


        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void ClearErrors()
        {
            ErrorMessage = string.Empty;
            HasError = false;
            EmailError = string.Empty;
            HasEmailError = false;
            PasswordError = string.Empty;
            HasPasswordError = false;
        }

        #endregion

        #region Event Handlers

        private void OnAuthenticationStateChanged(object? sender, AuthState state)
        {
            Dispatcher.Invoke(() =>
            {
                IsOnline = state.IsOnline;
                ConnectionStatus = state.IsOnline ? "Online" : "Offline";
            });
        }


        #endregion

        #region Connection Check

        private async Task CheckConnectionAsync()
        {
            try
            {
                var isOnline = await _authService.IsOnlineAsync();
                Dispatcher.Invoke(() =>
                {
                    IsOnline = isOnline;
                    ConnectionStatus = isOnline ? "Online" : "Offline";
                });
            }
            catch
            {
                Dispatcher.Invoke(() =>
                {
                    IsOnline = false;
                    ConnectionStatus = "Offline";
                });
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion


        #region Cleanup

        protected override void OnClosed(EventArgs e)
        {
            _authService.AuthenticationStateChanged -= OnAuthenticationStateChanged;
            _authService.UserLoggedIn -= OnUserLoggedIn;
            base.OnClosed(e);
        }

        #endregion
    }
}

