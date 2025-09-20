using ERP_AI.Core;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using ERP_AI.Data;
using System.Linq;

namespace ERP_AI.Desktop.ViewModels
{
    public class TransactionViewModel : BaseViewModel
    {
        public ObservableCollection<Transaction> Transactions { get; set; } = new();
        public ObservableCollection<TransactionDetail> Details { get; set; } = new();
        public ObservableCollection<Account> Accounts { get; set; } = new();
        public Transaction? SelectedTransaction { get; set; }
        public TransactionDetail? SelectedDetail { get; set; }
        public string TransactionReference { get; set; } = string.Empty;
        public string TransactionDescription { get; set; } = string.Empty;

        public RelayCommand AddTransactionCommand => new(_ => AddTransaction());
        public RelayCommand AddDetailCommand => new(_ => AddDetail());
        public RelayCommand DeleteDetailCommand => new(_ => DeleteDetail(), _ => SelectedDetail != null);
        public RelayCommand PostTransactionCommand => new(_ => PostTransaction());

        public void AddTransaction()
        {
            var transaction = new Transaction { Date = DateTime.Now };
            Transactions.Add(transaction);
            SelectedTransaction = transaction;
            Details.Clear();
        }

        public void AddDetail()
        {
            if (SelectedTransaction == null) return;
            var detail = new TransactionDetail
            {
                Id = Guid.NewGuid(),
                TransactionId = SelectedTransaction.Id,
                AccountId = Guid.Empty, // Will be set by user
                Amount = 0,
                IsDebit = true,
                Description = string.Empty
            };
            Details.Add(detail);
        }

        public void DeleteDetail()
        {
            if (SelectedDetail != null)
            {
                Details.Remove(SelectedDetail);
                SelectedDetail = null;
            }
        }

        public void PostTransaction()
        {
            if (SelectedTransaction == null || Details.Count < 2) return;
            decimal debitSum = 0, creditSum = 0;
            foreach (var d in Details)
            {
                if (d.IsDebit) debitSum += d.Amount;
                else creditSum += d.Amount;
            }
            if (debitSum != creditSum)
            {
                // Show validation error
                System.Windows.MessageBox.Show("Transaction is not balanced. Debits must equal credits.",
                                              "Validation Error",
                                              System.Windows.MessageBoxButton.OK,
                                              System.Windows.MessageBoxImage.Error);
                return;
            }

            // Persist transaction and update account balances
            PersistTransaction(SelectedTransaction, Details.ToList());
        }

        private void PersistTransaction(Transaction transaction, List<TransactionDetail> details)
        {
            try
            {
                // Update transaction with additional info
                transaction.UpdatedAt = DateTime.Now;
                transaction.Reference = TransactionReference;
                transaction.Description = TransactionDescription;

                // Associate details with transaction
                transaction.Details = details;

                // In a real application, you would:
                // 1. Save to database using UnitOfWork
                // 2. Update account balances
                // 3. Handle concurrency

                // For now, just update the transaction in the collection
                int index = Transactions.IndexOf(transaction);
                if (index >= 0)
                {
                    Transactions[index] = transaction;
                }

                // Update account balances (simplified)
                foreach (var detail in details)
                {
                    var account = Accounts.FirstOrDefault(a => a.Id == detail.AccountId);
                    if (account != null)
                    {
                        if (detail.IsDebit)
                            account.Balance += detail.Amount;
                        else
                            account.Balance -= detail.Amount;
                    }
                }

                // Clear current transaction and prepare for next
                Details.Clear();
                TransactionReference = string.Empty;
                TransactionDescription = string.Empty;

                System.Windows.MessageBox.Show("Transaction posted successfully!",
                                              "Success",
                                              System.Windows.MessageBoxButton.OK,
                                              System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error posting transaction: {ex.Message}",
                                              "Error",
                                              System.Windows.MessageBoxButton.OK,
                                              System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
