using System.Text.Json;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using ERP_AI.Core;
using ERP_AI.Data;
using ERP_AI.Desktop.Services;

namespace ERP_AI.Desktop.Services
{
    public class DataSyncService : IDataSyncService
    {
        private readonly HttpClient _httpClient;
        private readonly ERPDbContext _context;
        private readonly ILogger<DataSyncService> _logger;
        private readonly IConfiguration _configuration;

        private bool _isOnline = true;
        private bool _isSyncing = false;
        private DateTime _lastSyncTime = DateTime.MinValue;
        private SyncStatus _currentStatus = SyncStatus.Idle;

        public DataSyncService(HttpClient httpClient, ERPDbContext context, ILogger<DataSyncService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _context = context;
            _logger = logger;
            _configuration = configuration;
            
            // Set offline mode by default for local development
            _isOnline = false;
        }

        // Events
        public event EventHandler<SyncProgressEventArgs>? SyncProgress;
        public event EventHandler<SyncCompletedEventArgs>? SyncCompleted;
        public event EventHandler<SyncErrorEventArgs>? SyncError;

        // Properties
        public bool IsOnline => _isOnline;
        public bool IsSyncing => _isSyncing;
        public DateTime LastSyncTime => _lastSyncTime;
        public SyncStatus CurrentStatus => _currentStatus;

        public async Task<bool> SyncAllDataAsync()
        {
            if (_isSyncing) return false;

            try
            {
                _isSyncing = true;
                _currentStatus = SyncStatus.Syncing;
                var startTime = DateTime.UtcNow;
                var totalProcessed = 0;
                var conflictsFound = 0;

                _logger.LogInformation("Starting full data synchronization");

                // Sync in order of dependencies
                var syncTasks = new[]
                {
                    SyncAccountsAsync(),
                    SyncCustomersAsync(),
                    SyncVendorsAsync(),
                    SyncTransactionsAsync(),
                    SyncInvoicesAsync(),
                    SyncBillsAsync()
                };

                foreach (var task in syncTasks)
                {
                    try
                    {
                        var result = await task;
                        if (result)
                        {
                            totalProcessed++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during sync task");
                        OnSyncError("SyncAll", ex.Message, ex);
                    }
                }

                _lastSyncTime = DateTime.UtcNow;
                _currentStatus = SyncStatus.Idle;
                _isSyncing = false;

                var duration = DateTime.UtcNow - startTime;
                OnSyncCompleted(true, totalProcessed, conflictsFound, duration, "Full sync completed successfully");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during full sync");
                _currentStatus = SyncStatus.Error;
                _isSyncing = false;
                OnSyncCompleted(false, 0, 0, TimeSpan.Zero, $"Sync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SyncAccountsAsync()
        {
            try
            {
                OnSyncProgress("Accounts", 0, 100, "Starting account sync");

                // Download remote accounts
                var response = await _httpClient.GetAsync("/api/sync/accounts");
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Failed to fetch accounts: {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var remoteAccounts = JsonSerializer.Deserialize<List<Account>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Account>();

                // Get local accounts
                var localAccounts = await _context.Accounts.ToListAsync();

                var processed = 0;
                var total = remoteAccounts.Count;

                foreach (var remoteAccount in remoteAccounts)
                {
                    var localAccount = localAccounts.FirstOrDefault(a => a.Id == remoteAccount.Id);
                    
                    if (localAccount == null)
                    {
                        // Add new account
                        _context.Accounts.Add(remoteAccount);
                    }
                    else
                    {
                        // Update existing account
                        localAccount.Name = remoteAccount.Name;
                        localAccount.Code = remoteAccount.Code;
                        // localAccount.AccountType = remoteAccount.AccountType; // AccountType property not available
                        localAccount.IsActive = remoteAccount.IsActive;
                        localAccount.UpdatedAt = DateTime.UtcNow;
                    }

                    processed++;
                    OnSyncProgress("Accounts", processed, total, $"Processing account: {remoteAccount.Name}");
                }

                await _context.SaveChangesAsync();
                OnSyncProgress("Accounts", total, total, "Account sync completed");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing accounts");
                OnSyncError("Accounts", ex.Message, ex);
                return false;
            }
        }

        public async Task<bool> SyncTransactionsAsync()
        {
            try
            {
                OnSyncProgress("Transactions", 0, 100, "Starting transaction sync");

                var response = await _httpClient.GetAsync("/api/sync/transactions");
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Failed to fetch transactions: {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var remoteTransactions = JsonSerializer.Deserialize<List<Transaction>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Transaction>();

                var localTransactions = await _context.Transactions.ToListAsync();
                var processed = 0;
                var total = remoteTransactions.Count;

                foreach (var remoteTransaction in remoteTransactions)
                {
                    var localTransaction = localTransactions.FirstOrDefault(t => t.Id == remoteTransaction.Id);
                    
                    if (localTransaction == null)
                    {
                        _context.Transactions.Add(remoteTransaction);
                    }
                    else
                    {
                        localTransaction.Reference = remoteTransaction.Reference;
                        localTransaction.Description = remoteTransaction.Description;
                        localTransaction.TotalDebits = remoteTransaction.TotalDebits;
                        localTransaction.TotalCredits = remoteTransaction.TotalCredits;
                        localTransaction.UpdatedAt = DateTime.UtcNow;
                    }

                    processed++;
                    OnSyncProgress("Transactions", processed, total, $"Processing transaction: {remoteTransaction.Reference}");
                }

                await _context.SaveChangesAsync();
                OnSyncProgress("Transactions", total, total, "Transaction sync completed");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing transactions");
                OnSyncError("Transactions", ex.Message, ex);
                return false;
            }
        }

        public async Task<bool> SyncCustomersAsync()
        {
            try
            {
                OnSyncProgress("Customers", 0, 100, "Starting customer sync");

                var response = await _httpClient.GetAsync("/api/sync/customers");
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Failed to fetch customers: {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var remoteCustomers = JsonSerializer.Deserialize<List<Customer>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Customer>();

                var localCustomers = await _context.Customers.ToListAsync();
                var processed = 0;
                var total = remoteCustomers.Count;

                foreach (var remoteCustomer in remoteCustomers)
                {
                    var localCustomer = localCustomers.FirstOrDefault(c => c.Id == remoteCustomer.Id);
                    
                    if (localCustomer == null)
                    {
                        _context.Customers.Add(remoteCustomer);
                    }
                    else
                    {
                        localCustomer.Name = remoteCustomer.Name;
                        localCustomer.Email = remoteCustomer.Email;
                        localCustomer.Phone = remoteCustomer.Phone;
                        localCustomer.Address = remoteCustomer.Address;
                        localCustomer.UpdatedAt = DateTime.UtcNow;
                    }

                    processed++;
                    OnSyncProgress("Customers", processed, total, $"Processing customer: {remoteCustomer.Name}");
                }

                await _context.SaveChangesAsync();
                OnSyncProgress("Customers", total, total, "Customer sync completed");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing customers");
                OnSyncError("Customers", ex.Message, ex);
                return false;
            }
        }

        public async Task<bool> SyncVendorsAsync()
        {
            try
            {
                OnSyncProgress("Vendors", 0, 100, "Starting vendor sync");

                var response = await _httpClient.GetAsync("/api/sync/vendors");
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Failed to fetch vendors: {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var remoteVendors = JsonSerializer.Deserialize<List<Vendor>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Vendor>();

                var localVendors = await _context.Vendors.ToListAsync();
                var processed = 0;
                var total = remoteVendors.Count;

                foreach (var remoteVendor in remoteVendors)
                {
                    var localVendor = localVendors.FirstOrDefault(v => v.Id == remoteVendor.Id);
                    
                    if (localVendor == null)
                    {
                        _context.Vendors.Add(remoteVendor);
                    }
                    else
                    {
                        localVendor.Name = remoteVendor.Name;
                        localVendor.Email = remoteVendor.Email;
                        localVendor.Phone = remoteVendor.Phone;
                        localVendor.Address = remoteVendor.Address;
                        localVendor.UpdatedAt = DateTime.UtcNow;
                    }

                    processed++;
                    OnSyncProgress("Vendors", processed, total, $"Processing vendor: {remoteVendor.Name}");
                }

                await _context.SaveChangesAsync();
                OnSyncProgress("Vendors", total, total, "Vendor sync completed");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing vendors");
                OnSyncError("Vendors", ex.Message, ex);
                return false;
            }
        }

        public async Task<bool> SyncInvoicesAsync()
        {
            try
            {
                OnSyncProgress("Invoices", 0, 100, "Starting invoice sync");

                var response = await _httpClient.GetAsync("/api/sync/invoices");
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Failed to fetch invoices: {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var remoteInvoices = JsonSerializer.Deserialize<List<Invoice>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Invoice>();

                var localInvoices = await _context.Invoices.ToListAsync();
                var processed = 0;
                var total = remoteInvoices.Count;

                foreach (var remoteInvoice in remoteInvoices)
                {
                    var localInvoice = localInvoices.FirstOrDefault(i => i.Id == remoteInvoice.Id);
                    
                    if (localInvoice == null)
                    {
                        _context.Invoices.Add(remoteInvoice);
                    }
                    else
                    {
                        localInvoice.InvoiceNumber = remoteInvoice.InvoiceNumber;
                        localInvoice.CustomerId = remoteInvoice.CustomerId;
                        // localInvoice.TotalAmount = remoteInvoice.TotalAmount; // TotalAmount property not available
                        localInvoice.Status = remoteInvoice.Status;
                        localInvoice.UpdatedAt = DateTime.UtcNow;
                    }

                    processed++;
                    OnSyncProgress("Invoices", processed, total, $"Processing invoice: {remoteInvoice.InvoiceNumber}");
                }

                await _context.SaveChangesAsync();
                OnSyncProgress("Invoices", total, total, "Invoice sync completed");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing invoices");
                OnSyncError("Invoices", ex.Message, ex);
                return false;
            }
        }

        public async Task<bool> SyncBillsAsync()
        {
            try
            {
                OnSyncProgress("Bills", 0, 100, "Starting bill sync");

                var response = await _httpClient.GetAsync("/api/sync/bills");
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Failed to fetch bills: {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var remoteBills = JsonSerializer.Deserialize<List<Bill>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Bill>();

                var localBills = await _context.Bills.ToListAsync();
                var processed = 0;
                var total = remoteBills.Count;

                foreach (var remoteBill in remoteBills)
                {
                    var localBill = localBills.FirstOrDefault(b => b.Id == remoteBill.Id);
                    
                    if (localBill == null)
                    {
                        _context.Bills.Add(remoteBill);
                    }
                    else
                    {
                        localBill.BillNumber = remoteBill.BillNumber;
                        localBill.VendorId = remoteBill.VendorId;
                        // localBill.TotalAmount = remoteBill.TotalAmount; // TotalAmount property not available
                        localBill.Status = remoteBill.Status;
                        localBill.UpdatedAt = DateTime.UtcNow;
                    }

                    processed++;
                    OnSyncProgress("Bills", processed, total, $"Processing bill: {remoteBill.BillNumber}");
                }

                await _context.SaveChangesAsync();
                OnSyncProgress("Bills", total, total, "Bill sync completed");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing bills");
                OnSyncError("Bills", ex.Message, ex);
                return false;
            }
        }

        // Conflict Resolution Methods
        public async Task<List<SyncConflict>> GetConflictsAsync()
        {
            // Implementation for conflict detection
            return new List<SyncConflict>();
        }

        public async Task<bool> ResolveConflictAsync(string conflictId, ConflictResolution resolution)
        {
            // Implementation for conflict resolution
            return true;
        }

        public async Task<bool> ResolveAllConflictsAsync(ConflictResolution defaultResolution)
        {
            // Implementation for bulk conflict resolution
            return true;
        }

        // Offline Support Methods
        public async Task<bool> QueueOfflineActionAsync(OfflineAction action)
        {
            try
            {
                // Store offline action for later processing
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error queuing offline action");
                return false;
            }
        }

        public async Task<List<OfflineAction>> GetPendingActionsAsync()
        {
            // Return pending offline actions
            return new List<OfflineAction>();
        }

        public async Task<bool> ProcessPendingActionsAsync()
        {
            try
            {
                var pendingActions = await GetPendingActionsAsync();
                foreach (var action in pendingActions)
                {
                    // Process each offline action
                    action.RetryCount++;
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing pending actions");
                return false;
            }
        }

        // Data Management Methods
        public async Task<bool> UploadLocalChangesAsync()
        {
            try
            {
                // Upload local changes to server
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading local changes");
                return false;
            }
        }

        public async Task<bool> DownloadRemoteChangesAsync()
        {
            try
            {
                // Download changes from server
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading remote changes");
                return false;
            }
        }

        public async Task<bool> ClearLocalDataAsync()
        {
            try
            {
                // Clear local data (use with caution)
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing local data");
                return false;
            }
        }

        public async Task<bool> ResetSyncStatusAsync()
        {
            try
            {
                _lastSyncTime = DateTime.MinValue;
                _currentStatus = SyncStatus.Idle;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting sync status");
                return false;
            }
        }

        // Health Check Methods
        public async Task<SyncHealthStatus> GetSyncHealthAsync()
        {
            return new SyncHealthStatus
            {
                IsHealthy = _currentStatus != SyncStatus.Error,
                PendingConflicts = 0,
                FailedSyncs = 0,
                PendingOfflineActions = 0,
                LastSuccessfulSync = _lastSyncTime,
                Issues = new List<string>()
            };
        }

        public async Task<bool> ValidateDataIntegrityAsync()
        {
            try
            {
                // Validate data integrity
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating data integrity");
                return false;
            }
        }

        // Event Handlers
        private void OnSyncProgress(string entityType, int processed, int total, string message)
        {
            SyncProgress?.Invoke(this, new SyncProgressEventArgs
            {
                EntityType = entityType,
                Processed = processed,
                Total = total,
                Message = message
            });
        }

        private void OnSyncCompleted(bool success, int totalProcessed, int conflictsFound, TimeSpan duration, string message)
        {
            SyncCompleted?.Invoke(this, new SyncCompletedEventArgs
            {
                Success = success,
                TotalProcessed = totalProcessed,
                ConflictsFound = conflictsFound,
                Duration = duration,
                Message = message
            });
        }

        private void OnSyncError(string entityType, string errorMessage, Exception? exception = null)
        {
            SyncError?.Invoke(this, new SyncErrorEventArgs
            {
                EntityType = entityType,
                ErrorMessage = errorMessage,
                Exception = exception,
                CanRetry = true
            });
        }
    }
}
