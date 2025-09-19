using ERP_AI.Core;
using System;
using System.Collections.ObjectModel;
using ERP_AI.Data;

namespace ERP_AI.Desktop.ViewModels
{
    public class TransactionViewModel : BaseViewModel
    {
        public ObservableCollection<Transaction> Transactions { get; set; } = new();
        public ObservableCollection<TransactionDetail> Details { get; set; } = new();
        public Transaction? SelectedTransaction { get; set; }
        public RelayCommand AddTransactionCommand => new(_ => AddTransaction());
        public RelayCommand AddDetailCommand => new(_ => AddDetail());
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
            var detail = new TransactionDetail { TransactionId = SelectedTransaction.Id };
            Details.Add(detail);
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
            if (debitSum != creditSum) return; // Double-entry validation
            // TODO: Persist transaction and update account balances
        }
    }
}
