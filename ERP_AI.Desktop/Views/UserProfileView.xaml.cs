using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using ERP_AI.Desktop.Models;
using ERP_AI.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ERP_AI.Desktop.Views
{
    public partial class UserProfileView : Window, INotifyPropertyChanged
    {
        private readonly IAuthenticationService _authService;

        public UserProfileView(IAuthenticationService authService)
        {
            _authService = authService;
            
            InitializeComponent();
            DataContext = this;

            // Initialize commands
            SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
            ChangePasswordCommand = new AsyncRelayCommand(ChangePasswordAsync, CanChangePassword);

            // Load user data
            _ = Task.Run(LoadUserDataAsync);

            // Add password change handlers
            CurrentPasswordBox.PasswordChanged += (s, e) => ChangePasswordCommand.NotifyCanExecuteChanged();
            NewPasswordBox.PasswordChanged += (s, e) => ChangePasswordCommand.NotifyCanExecuteChanged();
            ConfirmPasswordBox.PasswordChanged += (s, e) => ChangePasswordCommand.NotifyCanExecuteChanged();
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
                    SaveCommand.NotifyCanExecuteChanged();
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
                    SaveCommand.NotifyCanExecuteChanged();
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
                    SaveCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private string _phoneNumber = string.Empty;
        public string PhoneNumber
        {
            get => _phoneNumber;
            set => SetProperty(ref _phoneNumber, value);
        }

        private string _company = string.Empty;
        public string Company
        {
            get => _company;
            set => SetProperty(ref _company, value);
        }

        private string _selectedTheme = "Light";
        public string SelectedTheme
        {
            get => _selectedTheme;
            set => SetProperty(ref _selectedTheme, value);
        }

        private string _selectedLanguage = "English";
        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set => SetProperty(ref _selectedLanguage, value);
        }

        private bool _emailNotifications = true;
        public bool EmailNotifications
        {
            get => _emailNotifications;
            set => SetProperty(ref _emailNotifications, value);
        }

        private bool _pushNotifications = true;
        public bool PushNotifications
        {
            get => _pushNotifications;
            set => SetProperty(ref _pushNotifications, value);
        }

        private bool _securityAlerts = true;
        public bool SecurityAlerts
        {
            get => _securityAlerts;
            set => SetProperty(ref _securityAlerts, value);
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

        private string _currentPasswordError = string.Empty;
        public string CurrentPasswordError
        {
            get => _currentPasswordError;
            set => SetProperty(ref _currentPasswordError, value);
        }

        private string _newPasswordError = string.Empty;
        public string NewPasswordError
        {
            get => _newPasswordError;
            set => SetProperty(ref _newPasswordError, value);
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

        private string _successMessage = string.Empty;
        public string SuccessMessage
        {
            get => _successMessage;
            set => SetProperty(ref _successMessage, value);
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

        private bool _hasCurrentPasswordError = false;
        public bool HasCurrentPasswordError
        {
            get => _hasCurrentPasswordError;
            set => SetProperty(ref _hasCurrentPasswordError, value);
        }

        private bool _hasNewPasswordError = false;
        public bool HasNewPasswordError
        {
            get => _hasNewPasswordError;
            set => SetProperty(ref _hasNewPasswordError, value);
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

        private bool _hasSuccess = false;
        public bool HasSuccess
        {
            get => _hasSuccess;
            set => SetProperty(ref _hasSuccess, value);
        }

        private bool _isLoading = false;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        #endregion

        #region Commands

        public IAsyncRelayCommand SaveCommand { get; }
        public IAsyncRelayCommand ChangePasswordCommand { get; }

        #endregion

        #region Command Implementations

        private async Task SaveAsync()
        {
            try
            {
                IsLoading = true;
                ClearErrors();

                if (!ValidateAll())
                {
                    return;
                }

                // Update user profile
                var updateRequest = new UserProfileUpdateRequest
                {
                    FirstName = FirstName.Trim(),
                    LastName = LastName.Trim(),
                    Email = Email.Trim(),
                    PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber.Trim(),
                    Company = string.IsNullOrWhiteSpace(Company) ? null : Company.Trim(),
                    Theme = SelectedTheme,
                    Language = SelectedLanguage,
                    EmailNotifications = EmailNotifications,
                    PushNotifications = PushNotifications,
                    SecurityAlerts = SecurityAlerts
                };

                var response = await _authService.UpdateUserProfileAsync(updateRequest);

                if (response.Success)
                {
                    SuccessMessage = "Profile updated successfully!";
                    HasSuccess = true;
                    HasError = false;
                }
                else
                {
                    ErrorMessage = response.ErrorMessage ?? "Failed to update profile. Please try again.";
                    HasError = true;
                    HasSuccess = false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while updating your profile. Please try again.";
                HasError = true;
                HasSuccess = false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(FirstName) &&
                   !string.IsNullOrWhiteSpace(LastName) &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   !IsLoading;
        }

        private async Task ChangePasswordAsync()
        {
            try
            {
                IsLoading = true;
                ClearPasswordErrors();

                if (!ValidatePasswords())
                {
                    return;
                }

                var changePasswordRequest = new ChangePasswordRequest
                {
                    CurrentPassword = CurrentPasswordBox.Password,
                    NewPassword = NewPasswordBox.Password
                };

                var response = await _authService.ChangeUserPasswordAsync(changePasswordRequest);

                if (response.Success)
                {
                    SuccessMessage = "Password changed successfully!";
                    HasSuccess = true;
                    HasError = false;
                    
                    // Clear password fields
                    CurrentPasswordBox.Password = string.Empty;
                    NewPasswordBox.Password = string.Empty;
                    ConfirmPasswordBox.Password = string.Empty;
                }
                else
                {
                    ErrorMessage = response.ErrorMessage ?? "Failed to change password. Please try again.";
                    HasError = true;
                    HasSuccess = false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while changing your password. Please try again.";
                HasError = true;
                HasSuccess = false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanChangePassword()
        {
            return !string.IsNullOrWhiteSpace(CurrentPasswordBox?.Password) &&
                   !string.IsNullOrWhiteSpace(NewPasswordBox?.Password) &&
                   !string.IsNullOrWhiteSpace(ConfirmPasswordBox?.Password) &&
                   !IsLoading;
        }

        #endregion

        #region Validation

        private bool ValidateAll()
        {
            var isValid = true;

            if (!ValidateFirstName()) isValid = false;
            if (!ValidateLastName()) isValid = false;
            if (!ValidateEmail()) isValid = false;

            return isValid;
        }

        private bool ValidateFirstName()
        {
            if (string.IsNullOrWhiteSpace(FirstName))
            {
                FirstNameError = "First name is required.";
                HasFirstNameError = true;
                return false;
            }

            if (FirstName.Length < 2)
            {
                FirstNameError = "First name must be at least 2 characters.";
                HasFirstNameError = true;
                return false;
            }

            FirstNameError = string.Empty;
            HasFirstNameError = false;
            return true;
        }

        private bool ValidateLastName()
        {
            if (string.IsNullOrWhiteSpace(LastName))
            {
                LastNameError = "Last name is required.";
                HasLastNameError = true;
                return false;
            }

            if (LastName.Length < 2)
            {
                LastNameError = "Last name must be at least 2 characters.";
                HasLastNameError = true;
                return false;
            }

            LastNameError = string.Empty;
            HasLastNameError = false;
            return true;
        }

        private bool ValidateEmail()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                EmailError = "Email address is required.";
                HasEmailError = true;
                return false;
            }

            if (!IsValidEmail(Email))
            {
                EmailError = "Please enter a valid email address.";
                HasEmailError = true;
                return false;
            }

            EmailError = string.Empty;
            HasEmailError = false;
            return true;
        }

        private bool ValidatePasswords()
        {
            var isValid = true;

            if (string.IsNullOrWhiteSpace(CurrentPasswordBox.Password))
            {
                CurrentPasswordError = "Current password is required.";
                HasCurrentPasswordError = true;
                isValid = false;
            }
            else
            {
                CurrentPasswordError = string.Empty;
                HasCurrentPasswordError = false;
            }

            if (string.IsNullOrWhiteSpace(NewPasswordBox.Password))
            {
                NewPasswordError = "New password is required.";
                HasNewPasswordError = true;
                isValid = false;
            }
            else if (NewPasswordBox.Password.Length < 8)
            {
                NewPasswordError = "New password must be at least 8 characters.";
                HasNewPasswordError = true;
                isValid = false;
            }
            else
            {
                NewPasswordError = string.Empty;
                HasNewPasswordError = false;
            }

            if (string.IsNullOrWhiteSpace(ConfirmPasswordBox.Password))
            {
                ConfirmPasswordError = "Please confirm your new password.";
                HasConfirmPasswordError = true;
                isValid = false;
            }
            else if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
            {
                ConfirmPasswordError = "Passwords do not match.";
                HasConfirmPasswordError = true;
                isValid = false;
            }
            else
            {
                ConfirmPasswordError = string.Empty;
                HasConfirmPasswordError = false;
            }

            return isValid;
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
            SuccessMessage = string.Empty;
            HasError = false;
            HasSuccess = false;
        }

        private void ClearPasswordErrors()
        {
            CurrentPasswordError = string.Empty;
            NewPasswordError = string.Empty;
            ConfirmPasswordError = string.Empty;
            HasCurrentPasswordError = false;
            HasNewPasswordError = false;
            HasConfirmPasswordError = false;
        }

        #endregion

        #region Data Loading

        private async Task LoadUserDataAsync()
        {
            try
            {
                IsLoading = true;

                // Get current user info
                var userInfo = await _authService.GetCurrentUserAsync();
                
                if (userInfo != null)
                {
                    Dispatcher.Invoke(() =>
                    {
                        FirstName = userInfo.FirstName ?? string.Empty;
                        LastName = userInfo.LastName ?? string.Empty;
                        Email = userInfo.Email ?? string.Empty;
                        PhoneNumber = userInfo.PhoneNumber ?? string.Empty;
                        Company = userInfo.Company ?? string.Empty;
                    });
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    ErrorMessage = "Failed to load user data. Please try again.";
                    HasError = true;
                });
            }
            finally
            {
                Dispatcher.Invoke(() =>
                {
                    IsLoading = false;
                });
            }
        }

        #endregion

        #region Event Handlers

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion

        #region INotifyPropertyChanged Implementation

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
    }
}
