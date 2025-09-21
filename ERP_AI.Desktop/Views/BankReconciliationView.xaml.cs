using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ERP_AI.Data;
using ERP_AI.Services;
using Microsoft.Win32;

namespace ERP_AI.Desktop.Views
{
    /// <summary>
    /// Interaction logic for BankReconciliationView.xaml
    /// </summary>
    public partial class BankReconciliationView : Window
    {
        private readonly IBankReconciliationService _bankReconciliationService;
        private readonly IUnitOfWork _unitOfWork;
        
        private ObservableCollection<BankStatementItem> _statementItems = new();
        private ObservableCollection<MatchCandidate> _matchCandidates = new();
        private BankStatementItem? _selectedItem;
        private BankReconciliation? _currentReconciliation;

        public BankReconciliationView(IBankReconciliationService bankReconciliationService, IUnitOfWork unitOfWork)
        {
            InitializeComponent();
            _bankReconciliationService = bankReconciliationService;
            _unitOfWork = unitOfWork;
            
            StatementItemsGrid.ItemsSource = _statementItems;
            MatchCandidatesGrid.ItemsSource = _matchCandidates;
            
            LoadSampleData();
        }

        private void LoadSampleData()
        {
            // Load sample bank statement items for demonstration
            _statementItems.Clear();
            
            var sampleItems = new List<BankStatementItem>
            {
                new BankStatementItem
                {
                    Id = Guid.NewGuid(),
                    TransactionDate = DateTime.Now.AddDays(-5),
                    Description = "DEPOSIT FROM CUSTOMER ABC",
                    Reference = "DEP001",
                    Amount = 1500.00m,
                    TransactionType = "Credit",
                    RunningBalance = 1500.00m,
                    MatchStatus = "Unmatched",
                    MatchConfidence = 0
                },
                new BankStatementItem
                {
                    Id = Guid.NewGuid(),
                    TransactionDate = DateTime.Now.AddDays(-4),
                    Description = "CHECK #1001",
                    Reference = "CHK1001",
                    Amount = -250.00m,
                    TransactionType = "Debit",
                    RunningBalance = 1250.00m,
                    MatchStatus = "Unmatched",
                    MatchConfidence = 0
                },
                new BankStatementItem
                {
                    Id = Guid.NewGuid(),
                    TransactionDate = DateTime.Now.AddDays(-3),
                    Description = "WIRE TRANSFER IN",
                    Reference = "WIRE001",
                    Amount = 5000.00m,
                    TransactionType = "Credit",
                    RunningBalance = 6250.00m,
                    MatchStatus = "AutoMatched",
                    MatchConfidence = 95
                },
                new BankStatementItem
                {
                    Id = Guid.NewGuid(),
                    TransactionDate = DateTime.Now.AddDays(-2),
                    Description = "ACH PAYMENT - VENDOR XYZ",
                    Reference = "ACH001",
                    Amount = -750.00m,
                    TransactionType = "Debit",
                    RunningBalance = 5500.00m,
                    MatchStatus = "ManualMatched",
                    MatchConfidence = 100
                },
                new BankStatementItem
                {
                    Id = Guid.NewGuid(),
                    TransactionDate = DateTime.Now.AddDays(-1),
                    Description = "MONTHLY FEE",
                    Reference = "FEE001",
                    Amount = -15.00m,
                    TransactionType = "Debit",
                    RunningBalance = 5485.00m,
                    MatchStatus = "Excluded",
                    MatchConfidence = 0
                }
            };

            foreach (var item in sampleItems)
            {
                _statementItems.Add(item);
            }

            UpdateStatus();
        }

        private void UpdateStatus()
        {
            var matchedCount = _statementItems.Count(i => i.IsMatched);
            var totalCount = _statementItems.Count;
            
            MatchCount.Text = $"{matchedCount}/{totalCount} items matched";
            
            // Update reconciliation summary
            BookBalance.Text = "$5,485.00";
            BankBalance.Text = "$5,485.00";
            OutstandingChecks.Text = "$250.00";
            OutstandingDeposits.Text = "$0.00";
            ReconciledBalance.Text = "$5,485.00";
            
            DiscrepancyText.Visibility = Visibility.Collapsed;
        }

        private async void ImportStatement_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Import Bank Statement",
                Filter = "CSV Files (*.csv)|*.csv|QIF Files (*.qif)|*.qif|OFX Files (*.ofx)|*.ofx|All Files (*.*)|*.*",
                DefaultExt = "csv"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    StatusText.Text = "Importing bank statement...";
                    
                    // Simulate import process
                    await Task.Delay(2000);
                    
                    // In real implementation, this would call the service
                    // var statement = await _bankReconciliationService.ImportCsvStatementAsync(openFileDialog.FileName, bankAccountId, options);
                    
                    StatusText.Text = $"Successfully imported {Path.GetFileName(openFileDialog.FileName)}";
                    MessageBox.Show($"Bank statement imported successfully!\n\nFile: {Path.GetFileName(openFileDialog.FileName)}\nItems: {_statementItems.Count}", 
                                  "Import Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    StatusText.Text = "Import failed";
                    MessageBox.Show($"Error importing bank statement:\n{ex.Message}", 
                                  "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void AutoMatch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "Running auto-matching algorithm...";
                
                // Simulate auto-matching process
                await Task.Delay(3000);
                
                // Simulate matching results
                var unmatchedItems = _statementItems.Where(i => !i.IsMatched).ToList();
                var matchedCount = 0;
                
                foreach (var item in unmatchedItems.Take(2)) // Match first 2 items
                {
                    item.IsMatched = true;
                    item.MatchStatus = "AutoMatched";
                    item.MatchConfidence = 85 + (matchedCount * 5); // Varying confidence scores
                    matchedCount++;
                }
                
                StatusText.Text = $"Auto-matching completed. {matchedCount} items matched.";
                UpdateStatus();
                
                MessageBox.Show($"Auto-matching completed!\n\nMatched: {matchedCount} items\nConfidence: 85-95%", 
                              "Auto-Match Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusText.Text = "Auto-matching failed";
                MessageBox.Show($"Error during auto-matching:\n{ex.Message}", 
                              "Auto-Match Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void CompleteReconciliation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show("Are you sure you want to complete this reconciliation?\n\nThis action cannot be undone.", 
                                           "Complete Reconciliation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    StatusText.Text = "Completing reconciliation...";
                    
                    // Simulate completion process
                    await Task.Delay(1500);
                    
                    StatusText.Text = "Reconciliation completed successfully";
                    
                    MessageBox.Show("Bank reconciliation completed successfully!\n\nAll matched items have been reconciled.", 
                                  "Reconciliation Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = "Reconciliation failed";
                MessageBox.Show($"Error completing reconciliation:\n{ex.Message}", 
                              "Reconciliation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StatementItemsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedItem = StatementItemsGrid.SelectedItem as BankStatementItem;
            
            if (_selectedItem != null)
            {
                SelectedItemDetails.Visibility = Visibility.Visible;
                SelectedDate.Text = $"Date: {_selectedItem.TransactionDate:yyyy-MM-dd}";
                SelectedDescription.Text = $"Description: {_selectedItem.Description}";
                SelectedAmount.Text = $"Amount: {_selectedItem.Amount:C2}";
                SelectedReference.Text = $"Reference: {_selectedItem.Reference}";
                
                // Load match candidates
                LoadMatchCandidates(_selectedItem);
            }
            else
            {
                SelectedItemDetails.Visibility = Visibility.Collapsed;
                _matchCandidates.Clear();
            }
        }

        private void LoadMatchCandidates(BankStatementItem item)
        {
            _matchCandidates.Clear();
            
            // Simulate match candidates
            var candidates = new List<MatchCandidate>
            {
                new MatchCandidate
                {
                    TransactionId = Guid.NewGuid(),
                    Transaction = new Transaction
                    {
                        Date = item.TransactionDate.AddDays(-1),
                        Description = item.Description + " (Similar)",
                        TotalDebits = item.Amount > 0 ? item.Amount : 0,
                        TotalCredits = item.Amount < 0 ? -item.Amount : 0,
                        Reference = item.Reference
                    },
                    MatchScore = 85,
                    MatchReason = "Similar description and amount"
                },
                new MatchCandidate
                {
                    TransactionId = Guid.NewGuid(),
                    Transaction = new Transaction
                    {
                        Date = item.TransactionDate,
                        Description = item.Description,
                        TotalDebits = (item.Amount + 0.01m) > 0 ? item.Amount + 0.01m : 0,
                        TotalCredits = (item.Amount + 0.01m) < 0 ? -(item.Amount + 0.01m) : 0,
                        Reference = item.Reference
                    },
                    MatchScore = 95,
                    MatchReason = "Exact match with minor amount difference"
                }
            };
            
            foreach (var candidate in candidates)
            {
                _matchCandidates.Add(candidate);
            }
        }

        private async void FindMatches_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem == null) return;
            
            try
            {
                StatusText.Text = "Finding match candidates...";
                
                // Simulate finding matches
                await Task.Delay(1000);
                
                LoadMatchCandidates(_selectedItem);
                
                StatusText.Text = $"Found {_matchCandidates.Count} potential matches";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error finding matches";
                MessageBox.Show($"Error finding matches:\n{ex.Message}", 
                              "Match Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ManualMatch_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem == null) return;
            
            var selectedCandidate = MatchCandidatesGrid.SelectedItem as MatchCandidate;
            if (selectedCandidate == null)
            {
                MessageBox.Show("Please select a match candidate first.", 
                              "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            try
            {
                _selectedItem.IsMatched = true;
                _selectedItem.MatchStatus = "ManualMatched";
                _selectedItem.MatchConfidence = selectedCandidate.MatchScore;
                
                UpdateStatus();
                StatusText.Text = "Item manually matched successfully";
                
                MessageBox.Show("Transaction matched successfully!", 
                              "Match Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusText.Text = "Manual match failed";
                MessageBox.Show($"Error matching transaction:\n{ex.Message}", 
                              "Match Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExcludeItem_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem == null) return;
            
            var result = MessageBox.Show("Are you sure you want to exclude this item from reconciliation?", 
                                       "Exclude Item", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                _selectedItem.MatchStatus = "Excluded";
                _selectedItem.IsMatched = false;
                
                UpdateStatus();
                StatusText.Text = "Item excluded from reconciliation";
            }
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Text == "Search transactions...")
            {
                SearchTextBox.Text = "";
                SearchTextBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                SearchTextBox.Text = "Search transactions...";
                SearchTextBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchTextBox.Text == "Search transactions...") return;
            
            var searchText = SearchTextBox.Text.ToLower();
            var filteredItems = _statementItems.Where(item => 
                item.Description.ToLower().Contains(searchText) ||
                item.Reference.ToLower().Contains(searchText) ||
                item.TransactionType.ToLower().Contains(searchText)
            ).ToList();
            
            StatementItemsGrid.ItemsSource = filteredItems;
        }

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedStatus = (StatusFilter.SelectedItem as ComboBoxItem)?.Content.ToString();
            
            IEnumerable<BankStatementItem> filteredItems = selectedStatus switch
            {
                "Unmatched" => _statementItems.Where(i => !i.IsMatched),
                "Matched" => _statementItems.Where(i => i.IsMatched),
                "Excluded" => _statementItems.Where(i => i.MatchStatus == "Excluded"),
                _ => _statementItems
            };
            
            StatementItemsGrid.ItemsSource = filteredItems;
        }

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "Search transactions...";
            SearchTextBox.Foreground = System.Windows.Media.Brushes.Gray;
            StatusFilter.SelectedIndex = 0;
            StatementItemsGrid.ItemsSource = _statementItems;
        }
    }
}
