using System.Collections.Concurrent;
using System.Text.Json;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using ERP_AI.Core;
using ERP_AI.Data;
using ERP_AI.Desktop.Models;

namespace ERP_AI.Desktop.Services
{
    public class IntegrationService : IIntegrationService
    {
        private readonly IAuthenticationService _authService;
        private readonly IDataSyncService _syncService;
        private readonly IErrorHandlingService _errorService;
        private readonly ERPDbContext _context;
        private readonly ILogger<IntegrationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        private bool _isInitialized = false;
        private bool _isOnline = true;
        private DateTime _lastSyncTime = DateTime.MinValue;
        private IntegrationStatus _status = IntegrationStatus.NotInitialized;
        private readonly ConcurrentDictionary<string, bool> _serviceHealth = new();
        private readonly List<ErrorLog> _errorLogs = new();
        private readonly Timer? _healthCheckTimer;
        private readonly Timer? _syncTimer;

        public IntegrationService(
            IAuthenticationService authService,
            IDataSyncService syncService,
            IErrorHandlingService errorService,
            ERPDbContext context,
            ILogger<IntegrationService> logger,
            IConfiguration configuration,
            HttpClient httpClient)
        {
            _authService = authService;
            _syncService = syncService;
            _errorService = errorService;
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClient;
            
            // Set offline mode by default for local development
            _isOnline = false;
            _status = IntegrationStatus.Offline;

            // Set up periodic health checks
            _healthCheckTimer = new Timer(PerformHealthCheck, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
            
            // Set up periodic sync
            _syncTimer = new Timer(PerformPeriodicSync, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(30));

            // Subscribe to events
            _authService.AuthenticationStateChanged += OnAuthenticationStateChanged;
            _syncService.SyncProgress += OnSyncProgress;
            _syncService.SyncCompleted += OnSyncCompleted;
            _syncService.SyncError += OnSyncError;
        }

        // Events
        public event EventHandler<IntegrationStatusChangedEventArgs>? StatusChanged;
        public event EventHandler<SyncProgressEventArgs>? SyncProgress;
        public event EventHandler<DataChangedEventArgs>? DataChanged;
        public event EventHandler<ErrorEventArgs>? ErrorOccurred;

        // Properties
        public bool IsInitialized => _isInitialized;
        public bool IsOnline => _isOnline;
        public DateTime LastSyncTime => _lastSyncTime;
        public IntegrationStatus Status => _status;

        public async Task<bool> InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Initializing integration service");
                SetStatus(IntegrationStatus.Initializing);

                // Initialize services
                if (!await InitializeServicesAsync())
                {
                    _logger.LogError("Failed to initialize services");
                    return false;
                }

                // Validate configuration
                if (!await ValidateConfigurationAsync())
                {
                    _logger.LogError("Configuration validation failed");
                    return false;
                }

                // Test connections
                if (!await TestConnectionsAsync())
                {
                    _logger.LogWarning("Connection test failed, continuing in offline mode");
                    _isOnline = false;
                }

                _isInitialized = true;
                SetStatus(_isOnline ? IntegrationStatus.Online : IntegrationStatus.Offline);

                _logger.LogInformation("Integration service initialized successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing integration service");
                SetStatus(IntegrationStatus.Error);
                await HandleErrorAsync(ex, "InitializeAsync");
                return false;
            }
        }

        public async Task<bool> InitializeServicesAsync()
        {
            try
            {
                // Initialize database
                await _context.Database.EnsureCreatedAsync();

                // Initialize authentication service
                if (_authService is RealAuthenticationService realAuth)
                {
                    await realAuth.CheckConnectionAsync();
                }

                // Initialize sync service
                await _syncService.ResetSyncStatusAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing services");
                return false;
            }
        }

        public async Task<bool> ValidateConfigurationAsync()
        {
            try
            {
                var requiredSettings = new[]
                {
                    "CloudApi:BaseUrl",
                    "Database:ConnectionString"
                };

                foreach (var setting in requiredSettings)
                {
                    var value = _configuration[setting];
                    if (string.IsNullOrEmpty(value))
                    {
                        _logger.LogError("Missing required configuration: {Setting}", setting);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating configuration");
                return false;
            }
        }

        public async Task<bool> TestConnectionsAsync()
        {
            try
            {
                var tasks = new[]
                {
                    TestCloudApiConnectionAsync(),
                    TestDatabaseConnectionAsync()
                };

                var results = await Task.WhenAll(tasks);
                return results.All(r => r);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing connections");
                return false;
            }
        }

        private async Task<bool> TestCloudApiConnectionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/health");
                var isHealthy = response.IsSuccessStatusCode;
                _serviceHealth["CloudAPI"] = isHealthy;
                return isHealthy;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cloud API connection test failed");
                _serviceHealth["CloudAPI"] = false;
                return false;
            }
        }

        private async Task<bool> TestDatabaseConnectionAsync()
        {
            try
            {
                await _context.Database.CanConnectAsync();
                _serviceHealth["Database"] = true;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Database connection test failed");
                _serviceHealth["Database"] = false;
                return false;
            }
        }

        // Authentication Integration
        public async Task<bool> AuthenticateAsync(string email, string password)
        {
            try
            {
                var request = new LoginRequest { Email = email, Password = password };
                var response = await _authService.LoginAsync(request);
                
                if (response.Success)
                {
                    _isOnline = true;
                    SetStatus(IntegrationStatus.Online);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex, "AuthenticateAsync");
                return false;
            }
        }

        public async Task<bool> RegisterAsync(string email, string password, string companyName)
        {
            try
            {
                var request = new RegisterRequest 
                { 
                    Email = email, 
                    Password = password, 
                    CompanyName = companyName 
                };
                var response = await _authService.RegisterAsync(request);
                
                if (response.Success)
                {
                    _isOnline = true;
                    SetStatus(IntegrationStatus.Online);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex, "RegisterAsync");
                return false;
            }
        }

        public async Task<bool> LogoutAsync()
        {
            try
            {
                var success = await _authService.LogoutAsync();
                if (success)
                {
                    SetStatus(IntegrationStatus.Offline);
                }
                return success;
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex, "LogoutAsync");
                return false;
            }
        }

        public async Task<bool> RefreshAuthenticationAsync()
        {
            try
            {
                if (_authService is RealAuthenticationService realAuth)
                {
                    return await realAuth.RefreshTokenAsync();
                }
                return true;
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex, "RefreshAuthenticationAsync");
                return false;
            }
        }

        // Data Synchronization
        public async Task<bool> SyncAllDataAsync()
        {
            try
            {
                SetStatus(IntegrationStatus.Syncing);
                var success = await _syncService.SyncAllDataAsync();
                
                if (success)
                {
                    _lastSyncTime = DateTime.UtcNow;
                    SetStatus(IntegrationStatus.Online);
                }
                else
                {
                    SetStatus(IntegrationStatus.Error);
                }

                return success;
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex, "SyncAllDataAsync");
                SetStatus(IntegrationStatus.Error);
                return false;
            }
        }

        public async Task<bool> SyncEntityAsync<T>(string entityType) where T : BaseEntity
        {
            try
            {
                return entityType.ToLower() switch
                {
                    "accounts" => await _syncService.SyncAccountsAsync(),
                    "transactions" => await _syncService.SyncTransactionsAsync(),
                    "customers" => await _syncService.SyncCustomersAsync(),
                    "vendors" => await _syncService.SyncVendorsAsync(),
                    "invoices" => await _syncService.SyncInvoicesAsync(),
                    "bills" => await _syncService.SyncBillsAsync(),
                    _ => false
                };
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex, $"SyncEntityAsync<{entityType}>");
                return false;
            }
        }

        public async Task<bool> UploadLocalChangesAsync()
        {
            try
            {
                return await _syncService.UploadLocalChangesAsync();
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex, "UploadLocalChangesAsync");
                return false;
            }
        }

        public async Task<bool> DownloadRemoteChangesAsync()
        {
            try
            {
                return await _syncService.DownloadRemoteChangesAsync();
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex, "DownloadRemoteChangesAsync");
                return false;
            }
        }

        public async Task<bool> ResolveConflictsAsync()
        {
            try
            {
                var conflicts = await _syncService.GetConflictsAsync();
                if (conflicts.Any())
                {
                    return await _syncService.ResolveAllConflictsAsync(ConflictResolution.UseRemote);
                }
                return true;
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex, "ResolveConflictsAsync");
                return false;
            }
        }

        // Real-time Updates
        public async Task<bool> StartRealTimeSyncAsync()
        {
            try
            {
                // Implementation for real-time sync (WebSockets, SignalR, etc.)
                _logger.LogInformation("Starting real-time sync");
                return true;
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex, "StartRealTimeSyncAsync");
                return false;
            }
        }

        public async Task<bool> StopRealTimeSyncAsync()
        {
            try
            {
                _logger.LogInformation("Stopping real-time sync");
                return true;
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex, "StopRealTimeSyncAsync");
                return false;
            }
        }

        public async Task<bool> SubscribeToUpdatesAsync(string entityType)
        {
            try
            {
                _logger.LogInformation("Subscribing to updates for {EntityType}", entityType);
                return true;
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex, "SubscribeToUpdatesAsync");
                return false;
            }
        }

        public async Task<bool> UnsubscribeFromUpdatesAsync(string entityType)
        {
            try
            {
                _logger.LogInformation("Unsubscribing from updates for {EntityType}", entityType);
                return true;
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex, "UnsubscribeFromUpdatesAsync");
                return false;
            }
        }

        // Offline Support
        public async Task<bool> EnableOfflineModeAsync()
        {
            try
            {
                _isOnline = false;
                SetStatus(IntegrationStatus.Offline);
                _logger.LogInformation("Offline mode enabled");
                return true;
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex, "EnableOfflineModeAsync");
                return false;
            }
        }

        public async Task<bool> DisableOfflineModeAsync()
        {
            try
            {
                _isOnline = true;
                SetStatus(IntegrationStatus.Online);
                _logger.LogInformation("Offline mode disabled");
                return true;
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex, "DisableOfflineModeAsync");
                return false;
            }
        }

        public async Task<bool> ProcessOfflineQueueAsync()
        {
            try
            {
                return await _syncService.ProcessPendingActionsAsync();
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex, "ProcessOfflineQueueAsync");
                return false;
            }
        }

        public async Task<bool> IsOfflineDataAvailableAsync()
        {
            try
            {
                var pendingActions = await _syncService.GetPendingActionsAsync();
                return pendingActions.Any();
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex, "IsOfflineDataAvailableAsync");
                return false;
            }
        }

        // Data Management
        public async Task<bool> BackupDataAsync(string backupPath)
        {
            try
            {
                _logger.LogInformation("Creating backup to {BackupPath}", backupPath);
                // Implementation for data backup
                return true;
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex, "BackupDataAsync");
                return false;
            }
        }

        public async Task<bool> RestoreDataAsync(string backupPath)
        {
            try
            {
                _logger.LogInformation("Restoring from backup {BackupPath}", backupPath);
                // Implementation for data restore
                return true;
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex, "RestoreDataAsync");
                return false;
            }
        }

        public async Task<bool> ClearLocalDataAsync()
        {
            try
            {
                return await _syncService.ClearLocalDataAsync();
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex, "ClearLocalDataAsync");
                return false;
            }
        }

        public async Task<bool> ValidateDataIntegrityAsync()
        {
            try
            {
                return await _syncService.ValidateDataIntegrityAsync();
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex, "ValidateDataIntegrityAsync");
                return false;
            }
        }

        // Performance Monitoring
        public async Task<PerformanceMetrics> GetPerformanceMetricsAsync()
        {
            return new PerformanceMetrics
            {
                AverageResponseTime = TimeSpan.FromMilliseconds(100),
                RequestsPerSecond = 10,
                ErrorRate = 5,
                MemoryUsage = GC.GetTotalMemory(false),
                ActiveConnections = 1,
                CachedItems = 0,
                Timestamp = DateTime.UtcNow
            };
        }

        public async Task<bool> OptimizePerformanceAsync()
        {
            try
            {
                await ClearCacheAsync();
                return true;
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex, "OptimizePerformanceAsync");
                return false;
            }
        }

        public async Task<bool> ClearCacheAsync()
        {
            try
            {
                GC.Collect();
                return true;
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex, "ClearCacheAsync");
                return false;
            }
        }

        // Error Handling
        public async Task<bool> HandleErrorAsync(Exception exception, string context)
        {
            try
            {
                await _errorService.HandleErrorAsync(exception, context);
                
                var errorLog = new ErrorLog
                {
                    Service = context,
                    ErrorMessage = exception.Message,
                    StackTrace = exception.StackTrace ?? string.Empty,
                    Timestamp = DateTime.UtcNow
                };

                lock (_errorLogs)
                {
                    _errorLogs.Add(errorLog);
                }

                ErrorOccurred?.Invoke(this, new ErrorEventArgs
                {
                    Service = context,
                    ErrorMessage = exception.Message,
                    Exception = exception,
                    CanRetry = _errorService.IsRetryableError(exception)
                });

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling error in {Context}", context);
                return false;
            }
        }

        public async Task<bool> RetryFailedOperationsAsync()
        {
            try
            {
                return await _syncService.ProcessPendingActionsAsync();
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex, "RetryFailedOperationsAsync");
                return false;
            }
        }

        public async Task<List<ErrorLog>> GetErrorLogsAsync()
        {
            lock (_errorLogs)
            {
                return _errorLogs.ToList();
            }
        }

        // Health Check
        public async Task<HealthStatus> GetHealthStatusAsync()
        {
            var services = new List<ServiceHealth>();
            
            foreach (var service in _serviceHealth)
            {
                services.Add(new ServiceHealth
                {
                    Name = service.Key,
                    IsHealthy = service.Value,
                    Status = service.Value ? "Healthy" : "Unhealthy",
                    ResponseTime = TimeSpan.Zero,
                    LastCheck = DateTime.UtcNow
                });
            }

            var isHealthy = services.All(s => s.IsHealthy);
            var issues = services.Where(s => !s.IsHealthy).Select(s => $"{s.Name}: {s.Status}").ToList();

            return new HealthStatus
            {
                IsHealthy = isHealthy,
                Services = services,
                Issues = issues,
                LastCheck = DateTime.UtcNow,
                OverallStatus = isHealthy ? "Healthy" : "Unhealthy"
            };
        }

        public async Task<bool> IsServiceHealthyAsync(string serviceName)
        {
            return _serviceHealth.GetValueOrDefault(serviceName, false);
        }

        public async Task<Dictionary<string, bool>> GetAllServicesHealthAsync()
        {
            return _serviceHealth.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        // Configuration
        public async Task<bool> UpdateConfigurationAsync(Dictionary<string, object> settings)
        {
            try
            {
                // Implementation for updating configuration
                return true;
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex, "UpdateConfigurationAsync");
                return false;
            }
        }

        public async Task<Dictionary<string, object>> GetConfigurationAsync()
        {
            return new Dictionary<string, object>();
        }

        public async Task<bool> ResetToDefaultsAsync()
        {
            try
            {
                // Implementation for resetting to defaults
                return true;
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex, "ResetToDefaultsAsync");
                return false;
            }
        }

        // Event Handlers
        private void OnAuthenticationStateChanged(object? sender, AuthState state)
        {
            _isOnline = state.IsOnline;
            SetStatus(_isOnline ? IntegrationStatus.Online : IntegrationStatus.Offline);
        }

        private void OnSyncProgress(object? sender, SyncProgressEventArgs e)
        {
            SyncProgress?.Invoke(this, e);
        }

        private void OnSyncCompleted(object? sender, SyncCompletedEventArgs e)
        {
            if (e.Success)
            {
                _lastSyncTime = DateTime.UtcNow;
                SetStatus(IntegrationStatus.Online);
            }
            else
            {
                SetStatus(IntegrationStatus.Error);
            }
        }

        private void OnSyncError(object? sender, SyncErrorEventArgs e)
        {
            ErrorOccurred?.Invoke(this, new ErrorEventArgs
            {
                Service = e.EntityType,
                ErrorMessage = e.ErrorMessage,
                Exception = e.Exception,
                CanRetry = e.CanRetry
            });
        }

        // Timer Callbacks
        private async void PerformHealthCheck(object? state)
        {
            try
            {
                await TestConnectionsAsync();
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex, "PerformHealthCheck");
            }
        }

        private async void PerformPeriodicSync(object? state)
        {
            try
            {
                if (_isOnline && _authService.IsAuthenticated)
                {
                    await SyncAllDataAsync();
                }
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex, "PerformPeriodicSync");
            }
        }

        // Helper Methods
        private void SetStatus(IntegrationStatus newStatus)
        {
            var oldStatus = _status;
            _status = newStatus;
            
            StatusChanged?.Invoke(this, new IntegrationStatusChangedEventArgs
            {
                OldStatus = oldStatus,
                NewStatus = newStatus,
                Message = $"Status changed from {oldStatus} to {newStatus}",
                Timestamp = DateTime.UtcNow
            });
        }

        public void Dispose()
        {
            _healthCheckTimer?.Dispose();
            _syncTimer?.Dispose();
        }
    }
}
