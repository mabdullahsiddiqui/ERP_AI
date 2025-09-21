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
    public partial class RegisterView : Window, INotifyPropertyChanged
    {
        private readonly IAuthenticationService _authService;
        private readonly INavigationService _navigationService;

        public RegisterView(IAuthenticationService authService, INavigationService navigationService)
        {
            _authService = authService;
            _navigationService = navigationService;
            
            InitializeComponent();
            DataContext = this;

            // Subscribe to authentication events
            _authService.AuthenticationStateChanged += OnAuthenticationStateChanged;
            _authService.UserLoggedIn += OnUserLoggedIn;

            // Initialize commands
            RegisterCommand = new AsyncRelayCommand(RegisterAsync, CanRegister);
            NavigateToLoginCommand = new RelayCommand(NavigateToLogin);
            ViewTermsCommand = new RelayCommand(ViewTerms);
            ViewPrivacyCommand = new RelayCommand(ViewPrivacy);

            // Check connection status
            _ = Task.Run(CheckConnectionAsync);

            // Set focus to first name field
            Loaded += (s, e) => FirstNameTextBox.Focus();
            
            // Add password change handlers
            PasswordBox.PasswordChanged += (s, e) => RegisterCommand.NotifyCanExecuteChanged();
            ConfirmPasswordBox.PasswordChanged += (s, e) => RegisterCommand.NotifyCanExecuteChanged();
        }

        #region Properties

        private string _firstName = string.Empty;
        public string FirstName
        {
            get => _firstName;
            set
            {
                if (SetProperty(ref _firstName, value))
                {
                    ValidateFirstName();
                    RegisterCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private string _lastName = string.Empty;
        public string LastName
        {
            get => _lastName;
            set
            {
                if (SetProperty(ref _lastName, value))
                {
                    ValidateLastName();
                    RegisterCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set
            {
                if (SetProperty(ref _email, value))
                {
                    ValidateEmail();
                    RegisterCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private string _phoneNumber = string.Empty;
        public string PhoneNumber
        {
            get => _phoneNumber;
            set => SetProperty(ref _phoneNumber, value);
        }

        private string _companyName = string.Empty;
        public string CompanyName
        {
            get => _companyName;
            set
            {
                if (SetProperty(ref _companyName, value))
                {
                    ValidateCompanyName();
                    RegisterCommand.NotifyCanExecuteChanged();
                }
            }
        }


        private bool _acceptTerms = false;
        public bool AcceptTerms
        {
            get => _acceptTerms;
            set
            {
                if (SetProperty(ref _acceptTerms, value))
                {
                    RegisterCommand.NotifyCanExecuteChanged();
                }
            }
        }

        // Error properties
        private string _firstNameError = string.Empty;
        public string FirstNameError
        {
            get => _firstNameError;
            set => SetProperty(ref _firstNameError, value);
        }

        private string _lastNameError = string.Empty;
        public string LastNameError
        {
            get => _lastNameError;
            set => SetProperty(ref _lastNameError, value);
        }

        private string _emailError = string.Empty;
        public string EmailError
        {
            get => _emailError;
            set => SetProperty(ref _emailError, value);
        }

        private string _companyNameError = string.Empty;
        public string CompanyNameError
        {
            get => _companyNameError;
            set => SetProperty(ref _companyNameError, value);
        }

        private string _passwordError = string.Empty;
        public string PasswordError
        {
            get => _passwordError;
            set => SetProperty(ref _passwordError, value);
        }

        private string _confirmPasswordError = string.Empty;
        public string ConfirmPasswordError
        {
            get => _confirmPasswordError;
            set => SetProperty(ref _confirmPasswordError, value);
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        // Error visibility properties
        private bool _hasFirstNameError = false;
        public bool HasFirstNameError
        {
            get => _hasFirstNameError;
            set => SetProperty(ref _hasFirstNameError, value);
        }

        private bool _hasLastNameError = false;
        public bool HasLastNameError
        {
            get => _hasLastNameError;
            set => SetProperty(ref _hasLastNameError, value);
        }

        private bool _hasEmailError = false;
        public bool HasEmailError
        {
            get => _hasEmailError;
            set => SetProperty(ref _hasEmailError, value);
        }

        private bool _hasCompanyNameError = false;
        public bool HasCompanyNameError
        {
            get => _hasCompanyNameError;
            set => SetProperty(ref _hasCompanyNameError, value);
        }

        private bool _hasPasswordError = false;
        public bool HasPasswordError
        {
            get => _hasPasswordError;
            set => SetProperty(ref _hasPasswordError, value);
        }

        private bool _hasConfirmPasswordError = false;
        public bool HasConfirmPasswordError
        {
            get => _hasConfirmPasswordError;
            set => SetProperty(ref _hasConfirmPasswordError, value);
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

        public IAsyncRelayCommand RegisterCommand { get; }
        public IRelayCommand NavigateToLoginCommand { get; }
        public IRelayCommand ViewTermsCommand { get; }
        public IRelayCommand ViewPrivacyCommand { get; }

        #endregion

        #region Command Implementations

        private async Task RegisterAsync()
        {
            try
            {
                IsLoading = true;
                ClearErrors();

                var request = new RegisterRequest
                {
                    FirstName = FirstName.Trim(),
                    LastName = LastName.Trim(),
                    Email = Email.Trim(),
                    PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber.Trim(),
                    CompanyName = CompanyName.Trim(),
                    Password = PasswordBox.Password,
                    ConfirmPassword = ConfirmPasswordBox.Password,
                    AcceptTerms = AcceptTerms
                };

                var response = await _authService.RegisterAsync(request);

                if (response.Success)
                {
                    // Success - the event handlers will handle showing the main window
                    // The UserLoggedIn or AuthenticationStateChanged event will be fired
                }
                else
                {
                    ErrorMessage = response.ErrorMessage ?? "Registration failed. Please try again.";
                    HasError = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred during registration. Please try again.";
                HasError = true;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanRegister()
        {
            return !string.IsNullOrWhiteSpace(FirstName) &&
                   !string.IsNullOrWhiteSpace(LastName) &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(CompanyName) &&
                   !string.IsNullOrWhiteSpace(PasswordBox?.Password) &&
                   !string.IsNullOrWhiteSpace(ConfirmPasswordBox?.Password) &&
                   AcceptTerms &&
                   !IsLoading;
        }

        private void NavigateToLogin()
        {
            var loginView = new LoginView(_authService, _navigationService);
            loginView.Show();
            Close();
        }

        private void ViewTerms()
        {
            MessageBox.Show("Terms of Service will be implemented in a future update.", 
                          "Terms of Service", 
                          MessageBoxButton.OK, 
                          MessageBoxImage.Information);
        }

        private void ViewPrivacy()
        {
            MessageBox.Show("Privacy Policy will be implemented in a future update.", 
                          "Privacy Policy", 
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

        private void ValidateFirstName()
        {
            if (string.IsNullOrWhiteSpace(FirstName))
            {
                FirstNameError = "First name is required";
                HasFirstNameError = true;
            }
            else
            {
                FirstNameError = string.Empty;
                HasFirstNameError = false;
            }
        }

        private void ValidateLastName()
        {
            if (string.IsNullOrWhiteSpace(LastName))
            {
                LastNameError = "Last name is required";
                HasLastNameError = true;
            }
            else
            {
                LastNameError = string.Empty;
                HasLastNameError = false;
            }
        }

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

        private void ValidateCompanyName()
        {
            if (string.IsNullOrWhiteSpace(CompanyName))
            {
                CompanyNameError = "Company name is required";
                HasCompanyNameError = true;
            }
            else
            {
                CompanyNameError = string.Empty;
                HasCompanyNameError = false;
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
            FirstNameError = string.Empty;
            HasFirstNameError = false;
            LastNameError = string.Empty;
            HasLastNameError = false;
            EmailError = string.Empty;
            HasEmailError = false;
            CompanyNameError = string.Empty;
            HasCompanyNameError = false;
            PasswordError = string.Empty;
            HasPasswordError = false;
            ConfirmPasswordError = string.Empty;
            HasConfirmPasswordError = false;
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

