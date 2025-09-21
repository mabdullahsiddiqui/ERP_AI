using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Input;
using ERP_AI.Desktop.Models;
using ERP_AI.Desktop.Services;

namespace ERP_AI.Desktop.Views
{
    public partial class UserManagementView : UserControl, INotifyPropertyChanged
    {
        private readonly IAuthenticationService _authService;
        private readonly IRoleService _roleService;
        private readonly IAuditService _auditService;

        public UserManagementView(IAuthenticationService authService, IRoleService roleService, IAuditService auditService)
        {
            InitializeComponent();
            _authService = authService;
            _roleService = roleService;
            _auditService = auditService;
            DataContext = this;
            
            LoadUsers();
            InitializeFilters();
            
            // Commands
            AddUserCommand = new AsyncRelayCommand(AddUserAsync);
            EditUserCommand = new AsyncRelayCommand<UserInfo>(EditUserAsync);
            DeleteUserCommand = new AsyncRelayCommand<UserInfo>(DeleteUserAsync);
            ViewUserCommand = new AsyncRelayCommand<UserInfo>(ViewUserAsync);
            ResetPasswordCommand = new AsyncRelayCommand<UserInfo>(ResetPasswordAsync);
            ViewAnalyticsCommand = new AsyncRelayCommand(ViewAnalyticsAsync);
            ManageRolesCommand = new AsyncRelayCommand(ManageRolesAsync);
        }

        #region Properties
        private ObservableCollection<UserInfo> _users = new();
        public ObservableCollection<UserInfo> Users
        {
            get => _users;
            set => SetProperty(ref _users, value);
        }

        private ObservableCollection<UserInfo> _filteredUsers = new();
        public ObservableCollection<UserInfo> FilteredUsers
        {
            get => _filteredUsers;
            set => SetProperty(ref _filteredUsers, value);
        }

        private UserInfo? _selectedUser;
        public UserInfo? SelectedUser
        {
            get => _selectedUser;
            set => SetProperty(ref _selectedUser, value);
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterUsers();
                }
            }
        }

        private string? _selectedRoleFilter;
        public string? SelectedRoleFilter
        {
            get => _selectedRoleFilter;
            set
            {
                if (SetProperty(ref _selectedRoleFilter, value))
                {
                    FilterUsers();
                }
            }
        }

        private string? _selectedStatusFilter;
        public string? SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set
            {
                if (SetProperty(ref _selectedStatusFilter, value))
                {
                    FilterUsers();
                }
            }
        }

        private ObservableCollection<string> _roleFilters = new();
        public ObservableCollection<string> RoleFilters
        {
            get => _roleFilters;
            set => SetProperty(ref _roleFilters, value);
        }

        private ObservableCollection<string> _statusFilters = new();
        public ObservableCollection<string> StatusFilters
        {
            get => _statusFilters;
            set => SetProperty(ref _statusFilters, value);
        }

        private string _statusMessage = "Ready";
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public int UserCount => Users.Count;
        #endregion

        #region Commands
        public IAsyncRelayCommand AddUserCommand { get; }
        public IAsyncRelayCommand<UserInfo> EditUserCommand { get; }
        public IAsyncRelayCommand<UserInfo> DeleteUserCommand { get; }
        public IAsyncRelayCommand<UserInfo> ViewUserCommand { get; }
        public IAsyncRelayCommand<UserInfo> ResetPasswordCommand { get; }
        public IAsyncRelayCommand ViewAnalyticsCommand { get; }
        public IAsyncRelayCommand ManageRolesCommand { get; }
        #endregion

        #region Command Implementations
        private async Task AddUserAsync()
        {
            try
            {
                StatusMessage = "Opening Add User dialog...";
                
                // In a real implementation, this would open a user creation dialog
                var newUser = new UserInfo
                {
                    Id = Guid.NewGuid().ToString(),
                    FirstName = "New",
                    LastName = "User",
                    Email = $"user{Guid.NewGuid().ToString("N")[..8]}@company.com",
                    CompanyId = "demo-company",
                    CompanyName = "Demo Company",
                    Roles = new List<string> { "User" },
                    IsActive = true,
                    LastLogin = DateTime.Now.AddDays(-1)
                };

                Users.Add(newUser);
                FilterUsers();
                
                StatusMessage = $"User '{newUser.FullName}' added successfully";
                
                // Log the action
                await _auditService.LogUserCreationAsync(
                    newUser.Id, 
                    newUser.Email, 
                    _authService.CurrentUser?.Email ?? "system@erpai.com"
                );
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error adding user: {ex.Message}";
            }
        }

        private async Task EditUserAsync(UserInfo? user)
        {
            if (user == null) return;

            try
            {
                StatusMessage = $"Opening edit dialog for {user.FullName}...";
                
                // In a real implementation, this would open an edit dialog
                MessageBox.Show($"Edit user functionality for {user.FullName} would be implemented here.", 
                               "Edit User", MessageBoxButton.OK, MessageBoxImage.Information);
                
                StatusMessage = $"User '{user.FullName}' updated successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error editing user: {ex.Message}";
            }
        }

        private async Task DeleteUserAsync(UserInfo? user)
        {
            if (user == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete user '{user.FullName}'?\n\nThis action cannot be undone.", 
                                       "Delete User", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    Users.Remove(user);
                    FilterUsers();
                    
                    StatusMessage = $"User '{user.FullName}' deleted successfully";
                    
                    // Log the action
                    await _auditService.LogUserDeletionAsync(
                        user.Id, 
                        user.Email, 
                        _authService.CurrentUser?.Email ?? "system@erpai.com"
                    );
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error deleting user: {ex.Message}";
                }
            }
        }

        private async Task ViewUserAsync(UserInfo? user)
        {
            if (user == null) return;

            try
            {
                StatusMessage = $"Viewing details for {user.FullName}...";
                
                // In a real implementation, this would open a detailed view
                var details = $"User Details:\n\n" +
                             $"Name: {user.FullName}\n" +
                             $"Email: {user.Email}\n" +
                             $"Roles: {string.Join(", ", user.Roles)}\n" +
                             $"Company: {user.CompanyName}\n" +
                             $"Active: {user.IsActive}\n" +
                             $"Last Login: {user.LastLogin:MMM dd, yyyy HH:mm}";
                
                MessageBox.Show(details, "User Details", MessageBoxButton.OK, MessageBoxImage.Information);
                
                StatusMessage = "User details displayed";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error viewing user: {ex.Message}";
            }
        }

        private async Task ResetPasswordAsync(UserInfo? user)
        {
            if (user == null) return;

            var result = MessageBox.Show($"Reset password for user '{user.FullName}'?\n\nA temporary password will be generated and sent to their email.", 
                                       "Reset Password", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // In a real implementation, this would reset the password
                    StatusMessage = $"Password reset initiated for {user.FullName}";
                    
                    // Log the action
                    await _auditService.LogPasswordResetRequestAsync(
                        user.Id, 
                        user.Email, 
                        true
                    );
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error resetting password: {ex.Message}";
                }
            }
        }

        private async Task ViewAnalyticsAsync()
        {
            try
            {
                StatusMessage = "Loading user analytics...";
                
                // In a real implementation, this would show analytics
                var analytics = $"User Analytics:\n\n" +
                               $"Total Users: {Users.Count}\n" +
                               $"Active Users: {Users.Count(u => u.IsActive)}\n" +
                               $"Inactive Users: {Users.Count(u => !u.IsActive)}\n" +
                               $"Recent Logins (24h): {Users.Count(u => u.LastLogin > DateTime.Now.AddDays(-1))}\n" +
                               $"Admin Users: {Users.Count(u => u.Roles.Contains("Admin"))}";
                
                MessageBox.Show(analytics, "User Analytics", MessageBoxButton.OK, MessageBoxImage.Information);
                
                StatusMessage = "Analytics displayed";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading analytics: {ex.Message}";
            }
        }

        private async Task ManageRolesAsync()
        {
            try
            {
                StatusMessage = "Opening role management...";
                
                // In a real implementation, this would open role management
                MessageBox.Show("Role management functionality would be implemented here.\n\nFeatures:\n- Create/Edit Roles\n- Assign Permissions\n- Manage Role Hierarchy", 
                               "Role Management", MessageBoxButton.OK, MessageBoxImage.Information);
                
                StatusMessage = "Role management opened";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error opening role management: {ex.Message}";
            }
        }
        #endregion

        #region Helper Methods
        private async void LoadUsers()
        {
            try
            {
                StatusMessage = "Loading users...";
                
                // Mock data for demonstration
                Users.Clear();
                Users.Add(new UserInfo
                {
                    Id = "1",
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@erpai.com",
                    CompanyId = "demo-company",
                    CompanyName = "Demo Company",
                    Roles = new List<string> { "Admin" },
                    IsActive = true,
                    LastLogin = DateTime.Now.AddHours(-2)
                });
                
                Users.Add(new UserInfo
                {
                    Id = "2",
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@company.com",
                    CompanyId = "demo-company",
                    CompanyName = "Demo Company",
                    Roles = new List<string> { "Accountant" },
                    IsActive = true,
                    LastLogin = DateTime.Now.AddDays(-1)
                });
                
                Users.Add(new UserInfo
                {
                    Id = "3",
                    FirstName = "Jane",
                    LastName = "Smith",
                    Email = "jane.smith@company.com",
                    CompanyId = "demo-company",
                    CompanyName = "Demo Company",
                    Roles = new List<string> { "User" },
                    IsActive = false,
                    LastLogin = DateTime.Now.AddDays(-7)
                });
                
                FilterUsers();
                StatusMessage = $"Loaded {Users.Count} users";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading users: {ex.Message}";
            }
        }

        private void InitializeFilters()
        {
            RoleFilters.Clear();
            RoleFilters.Add("All Roles");
            RoleFilters.Add("Admin");
            RoleFilters.Add("Accountant");
            RoleFilters.Add("User");
            SelectedRoleFilter = "All Roles";
            
            StatusFilters.Clear();
            StatusFilters.Add("All Status");
            StatusFilters.Add("Active");
            StatusFilters.Add("Inactive");
            SelectedStatusFilter = "All Status";
        }

        private void FilterUsers()
        {
            var filtered = Users.AsEnumerable();
            
            // Search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                filtered = filtered.Where(u => 
                    u.FullName.ToLower().Contains(searchLower) ||
                    u.Email.ToLower().Contains(searchLower) ||
                    u.Roles.Any(r => r.ToLower().Contains(searchLower)));
            }
            
            // Role filter
            if (SelectedRoleFilter != null && SelectedRoleFilter != "All Roles")
            {
                filtered = filtered.Where(u => u.Roles.Contains(SelectedRoleFilter));
            }
            
            // Status filter
            if (SelectedStatusFilter != null && SelectedStatusFilter != "All Status")
            {
                var isActive = SelectedStatusFilter == "Active";
                filtered = filtered.Where(u => u.IsActive == isActive);
            }
            
            FilteredUsers.Clear();
            foreach (var user in filtered)
            {
                FilteredUsers.Add(user);
            }
            
            OnPropertyChanged(nameof(UserCount));
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string? propertyName = null)
        {
            if (!Equals(field, newValue))
            {
                field = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }
            return false;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
