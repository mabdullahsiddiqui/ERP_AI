using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ERP_AI.Desktop.Views
{
    /// <summary>
    /// Interaction logic for ModernForm.xaml
    /// </summary>
    public partial class ModernForm : UserControl, INotifyPropertyChanged
    {
        private System.Timers.Timer _autoSaveTimer;
        private bool _isDirty;
        private bool _isValid;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsDirty
        {
            get => _isDirty;
            set
            {
                _isDirty = value;
                OnPropertyChanged();
                UpdateAutoSaveStatus();
            }
        }

        public bool IsValid
        {
            get => _isValid;
            set
            {
                _isValid = value;
                OnPropertyChanged();
                UpdateValidationStatus();
            }
        }

        public ModernForm()
        {
            InitializeComponent();
            InitializeAutoSave();
            SetupValidation();
        }

        private void InitializeAutoSave()
        {
            _autoSaveTimer = new System.Timers.Timer(30000); // Auto-save every 30 seconds
            _autoSaveTimer.Elapsed += OnAutoSaveTimer;
            _autoSaveTimer.Start();
        }

        private void SetupValidation()
        {
            // Add event handlers for validation
            NameTextBox.TextChanged += OnFieldChanged;
            TypeComboBox.SelectionChanged += OnFieldChanged;
            EmailTextBox.TextChanged += OnFieldChanged;
            PhoneTextBox.TextChanged += OnFieldChanged;
            
            // Initial validation
            ValidateForm();
        }

        private void OnFieldChanged(object sender, EventArgs e)
        {
            IsDirty = true;
            ValidateForm();
        }

        private void OnAutoSaveTimer(object sender, ElapsedEventArgs e)
        {
            if (IsDirty)
            {
                Dispatcher.Invoke(() =>
                {
                    SaveDraft();
                });
            }
        }

        private void ValidateForm()
        {
            bool isValid = true;
            string errorMessage = "";

            // Validate required fields
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                ShowFieldError(NameError, "Name is required");
                isValid = false;
            }
            else
            {
                HideFieldError(NameError);
            }

            if (TypeComboBox.SelectedItem == null)
            {
                ShowFieldError(TypeError, "Type is required");
                isValid = false;
            }
            else
            {
                HideFieldError(TypeError);
            }

            // Validate email format if provided
            if (!string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                if (!IsValidEmail(EmailTextBox.Text))
                {
                    ShowFieldError(EmailError, "Invalid email format");
                    isValid = false;
                }
                else
                {
                    HideFieldError(EmailError);
                }
            }
            else
            {
                HideFieldError(EmailError);
            }

            // Validate phone format if provided
            if (!string.IsNullOrWhiteSpace(PhoneTextBox.Text))
            {
                if (!IsValidPhone(PhoneTextBox.Text))
                {
                    ShowFieldError(PhoneError, "Invalid phone format");
                    isValid = false;
                }
                else
                {
                    HideFieldError(PhoneError);
                }
            }
            else
            {
                HideFieldError(PhoneError);
            }

            IsValid = isValid;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhone(string phone)
        {
            try
            {
                var regex = new Regex(@"^[\+]?[1-9][\d]{0,15}$");
                return regex.IsMatch(phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", ""));
            }
            catch
            {
                return false;
            }
        }

        private void ShowFieldError(TextBlock errorTextBlock, string message)
        {
            errorTextBlock.Text = message;
            errorTextBlock.Visibility = Visibility.Visible;
        }

        private void HideFieldError(TextBlock errorTextBlock)
        {
            errorTextBlock.Visibility = Visibility.Collapsed;
        }

        private void UpdateAutoSaveStatus()
        {
            if (IsDirty)
            {
                AutoSaveStatus.Text = "Unsaved changes";
                AutoSaveIndicator.Fill = new SolidColorBrush(Colors.Orange);
            }
            else
            {
                AutoSaveStatus.Text = "Auto-saved";
                AutoSaveIndicator.Fill = new SolidColorBrush(Colors.Green);
            }
        }

        private void UpdateValidationStatus()
        {
            if (IsValid)
            {
                ValidationStatus.Text = "Form is valid";
                ValidationStatus.Foreground = new SolidColorBrush(Colors.Green);
            }
            else
            {
                ValidationStatus.Text = "Please fix validation errors";
                ValidationStatus.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        private void SaveDraft()
        {
            // Implement draft saving logic
            IsDirty = false;
            AutoSaveStatus.Text = "Draft saved";
            AutoSaveIndicator.Fill = new SolidColorBrush(Colors.Blue);
        }

        public void Save()
        {
            if (IsValid)
            {
                // Implement save logic
                IsDirty = false;
                MessageBox.Show("Form saved successfully!", "Save", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Please fix validation errors before saving.", "Validation Error", 
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void Cancel()
        {
            if (IsDirty)
            {
                var result = MessageBox.Show("You have unsaved changes. Are you sure you want to cancel?", 
                                           "Unsaved Changes", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    // Reset form or close
                    ResetForm();
                }
            }
        }

        private void ResetForm()
        {
            // Reset all form fields
            NameTextBox.Clear();
            TypeComboBox.SelectedIndex = -1;
            EmailTextBox.Clear();
            PhoneTextBox.Clear();
            Address1TextBox.Clear();
            Address2TextBox.Clear();
            CityTextBox.Clear();
            StateTextBox.Clear();
            PostalCodeTextBox.Clear();
            CountryComboBox.SelectedIndex = -1;
            CreditLimitTextBox.Clear();
            PaymentTermsComboBox.SelectedIndex = -1;
            TaxIdTextBox.Clear();
            NotesTextBox.Clear();
            TagsTextBox.Clear();
            IsActiveCheckBox.IsChecked = true;
            EmailNotificationsCheckBox.IsChecked = true;
            AutoRemindersCheckBox.IsChecked = false;
            
            IsDirty = false;
            ValidateForm();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Cleanup will be handled by the parent window
    }
}
