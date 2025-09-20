using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ERP_AI.Data;
using Microsoft.EntityFrameworkCore;

namespace ERP_AI.Services
{
    public class BankReconciliationService : IBankReconciliationService
    {
        private readonly ERPDbContext _context;
        private readonly IUnitOfWork _unitOfWork;

        public BankReconciliationService(ERPDbContext context, IUnitOfWork unitOfWork)
        {
            _context = context;
            _unitOfWork = unitOfWork;
        }

        // Bank Statement Import
        public async Task<BankStatement> ImportBankStatementAsync(string filePath, Guid bankAccountId, ImportTemplate template)
        {
            var content = await File.ReadAllTextAsync(filePath);
            var items = await ParseStatementItemsAsync(content, template);
            
            var statement = new BankStatement
            {
                BankAccountId = bankAccountId,
                StatementDate = DateTime.UtcNow,
                StartDate = items.Min(i => i.TransactionDate),
                EndDate = items.Max(i => i.TransactionDate),
                OpeningBalance = items.FirstOrDefault()?.RunningBalance - items.FirstOrDefault()?.Amount ?? 0,
                ClosingBalance = items.LastOrDefault()?.RunningBalance ?? 0,
                ImportSource = template.FileFormat,
                FileName = Path.GetFileName(filePath),
                Status = "Imported",
                Items = items
            };

            _context.BankStatements.Add(statement);
            await _context.SaveChangesAsync();
            
            return statement;
        }

        public async Task<BankStatement> ImportCsvStatementAsync(string filePath, Guid bankAccountId, CsvImportOptions options)
        {
            var lines = await File.ReadAllLinesAsync(filePath);
            var items = new List<BankStatementItem>();
            decimal runningBalance = 0;

            for (int i = options.HasHeader ? 1 : 0; i < lines.Length; i++)
            {
                var columns = ParseCsvLine(lines[i]);
                if (columns.Length < 3) continue;

                var item = new BankStatementItem
                {
                    TransactionDate = DateTime.ParseExact(columns[options.DateColumn], options.DateFormat, CultureInfo.InvariantCulture),
                    Description = columns[options.DescriptionColumn],
                    Amount = ParseDecimal(columns[options.AmountColumn], options.DecimalSeparator),
                    Reference = options.ReferenceColumn >= 0 && options.ReferenceColumn < columns.Length ? columns[options.ReferenceColumn] : "",
                    TransactionType = DetermineTransactionType(columns[options.AmountColumn], options.CreditIndicator, options.DebitIndicator)
                };

                runningBalance += item.Amount;
                item.RunningBalance = runningBalance;
                items.Add(item);
            }

            var statement = new BankStatement
            {
                BankAccountId = bankAccountId,
                StatementDate = DateTime.UtcNow,
                StartDate = items.Min(i => i.TransactionDate),
                EndDate = items.Max(i => i.TransactionDate),
                OpeningBalance = items.FirstOrDefault()?.RunningBalance - items.FirstOrDefault()?.Amount ?? 0,
                ClosingBalance = items.LastOrDefault()?.RunningBalance ?? 0,
                ImportSource = "CSV",
                FileName = Path.GetFileName(filePath),
                Status = "Imported",
                Items = items
            };

            _context.BankStatements.Add(statement);
            await _context.SaveChangesAsync();
            
            return statement;
        }

        public async Task<BankStatement> ImportQifStatementAsync(string filePath, Guid bankAccountId)
        {
            // QIF format parsing implementation
            var content = await File.ReadAllTextAsync(filePath);
            var items = ParseQifContent(content);
            
            var statement = new BankStatement
            {
                BankAccountId = bankAccountId,
                StatementDate = DateTime.UtcNow,
                StartDate = items.Min(i => i.TransactionDate),
                EndDate = items.Max(i => i.TransactionDate),
                OpeningBalance = 0,
                ClosingBalance = items.Sum(i => i.Amount),
                ImportSource = "QIF",
                FileName = Path.GetFileName(filePath),
                Status = "Imported",
                Items = items
            };

            _context.BankStatements.Add(statement);
            await _context.SaveChangesAsync();
            
            return statement;
        }

        public async Task<BankStatement> ImportOfxStatementAsync(string filePath, Guid bankAccountId)
        {
            // OFX format parsing implementation
            var content = await File.ReadAllTextAsync(filePath);
            var items = ParseOfxContent(content);
            
            var statement = new BankStatement
            {
                BankAccountId = bankAccountId,
                StatementDate = DateTime.UtcNow,
                StartDate = items.Min(i => i.TransactionDate),
                EndDate = items.Max(i => i.TransactionDate),
                OpeningBalance = 0,
                ClosingBalance = items.Sum(i => i.Amount),
                ImportSource = "OFX",
                FileName = Path.GetFileName(filePath),
                Status = "Imported",
                Items = items
            };

            _context.BankStatements.Add(statement);
            await _context.SaveChangesAsync();
            
            return statement;
        }

        public async Task<List<BankStatementItem>> ParseStatementItemsAsync(string content, ImportTemplate template)
        {
            var items = new List<BankStatementItem>();
            
            // Apply template rules and field mappings
            var lines = content.Split('\n');
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                
                var item = new BankStatementItem();
                
                // Apply import rules
                foreach (var rule in template.Rules.Where(r => r.IsActive))
                {
                    if (Regex.IsMatch(line, rule.Pattern))
                    {
                        switch (rule.Action)
                        {
                            case "SetCategory":
                                item.Category = rule.Value;
                                break;
                            case "SetDescription":
                                item.Description = rule.Value;
                                break;
                        }
                    }
                }
                
                items.Add(item);
            }
            
            return items;
        }

        // Transaction Matching
        public async Task<List<MatchResult>> AutoMatchTransactionsAsync(Guid statementId)
        {
            var statement = await _context.BankStatements
                .Include(s => s.Items)
                .FirstOrDefaultAsync(s => s.Id == statementId);
            
            if (statement == null) return new List<MatchResult>();

            var unmatchedItems = statement.Items.Where(i => !i.IsMatched).ToList();
            var results = new List<MatchResult>();

            foreach (var item in unmatchedItems)
            {
                var candidates = await GetMatchCandidatesAsync(item.Id);
                var bestMatch = candidates.OrderByDescending(c => c.MatchScore).FirstOrDefault();
                
                if (bestMatch != null && bestMatch.MatchScore >= 80m) // 80% confidence threshold
                {
                    var matchResult = await MatchTransactionAsync(item.Id, bestMatch.TransactionId);
                    results.Add(matchResult);
                }
            }

            return results;
        }

        public async Task<MatchResult> MatchTransactionAsync(Guid statementItemId, Guid transactionId)
        {
            var statementItem = await _context.BankStatementItems.FindAsync(statementItemId);
            var transaction = await _context.Transactions.FindAsync(transactionId);
            
            if (statementItem == null || transaction == null)
                throw new ArgumentException("Statement item or transaction not found");

            var matchScore = await CalculateMatchScoreAsync(statementItem, transaction);
            
            statementItem.IsMatched = true;
            statementItem.MatchedTransactionId = transactionId;
            statementItem.MatchStatus = "AutoMatched";
            statementItem.MatchConfidence = matchScore;

            await _context.SaveChangesAsync();

            return new MatchResult
            {
                StatementItemId = statementItemId,
                TransactionId = transactionId,
                MatchScore = matchScore,
                MatchMethod = "Auto",
                IsExactMatch = matchScore >= 95
            };
        }

        public async Task<List<MatchCandidate>> GetMatchCandidatesAsync(Guid statementItemId)
        {
            var statementItem = await _context.BankStatementItems.FindAsync(statementItemId);
            if (statementItem == null) return new List<MatchCandidate>();

            var transactions = await _context.Transactions
                .Where(t => t.Date >= statementItem.TransactionDate.AddDays(-7) && 
                           t.Date <= statementItem.TransactionDate.AddDays(7))
                .ToListAsync();

            var candidates = new List<MatchCandidate>();

            foreach (var transaction in transactions)
            {
                var score = await CalculateMatchScoreAsync(statementItem, transaction);
                if (score >= 50m) // Minimum 50% confidence
                {
                    candidates.Add(new MatchCandidate
                    {
                        TransactionId = transaction.Id,
                        Transaction = transaction,
                        MatchScore = score,
                        MatchReason = GenerateMatchReason(statementItem, transaction, score)
                    });
                }
            }

            return candidates.OrderByDescending(c => c.MatchScore).ToList();
        }

        public async Task<decimal> CalculateMatchScoreAsync(BankStatementItem item, Transaction transaction)
        {
            decimal score = 0;
            var factors = new List<string>();

            // Amount matching (40% weight)
            var transactionAmount = transaction.TotalDebits - transaction.TotalCredits;
            if (Math.Abs(item.Amount - transactionAmount) <= 0.01m)
            {
                score += 40;
                factors.Add("Exact amount match");
            }
            else if (Math.Abs(item.Amount - transactionAmount) <= 1.00m)
            {
                score += 30;
                factors.Add("Close amount match");
            }

            // Date matching (30% weight)
            var daysDiff = Math.Abs((item.TransactionDate - transaction.Date).Days);
            if (daysDiff == 0)
            {
                score += 30;
                factors.Add("Exact date match");
            }
            else if (daysDiff <= 3)
            {
                score += 20;
                factors.Add("Close date match");
            }

            // Description matching (20% weight)
            var descriptionSimilarity = CalculateStringSimilarity(item.Description, transaction.Description);
            score += descriptionSimilarity * 20;
            if (descriptionSimilarity > 0.8m)
                factors.Add("Description similarity");

            // Reference matching (10% weight)
            if (!string.IsNullOrEmpty(item.Reference) && !string.IsNullOrEmpty(transaction.Reference))
            {
                if (item.Reference.Equals(transaction.Reference, StringComparison.OrdinalIgnoreCase))
                {
                    score += 10;
                    factors.Add("Reference match");
                }
            }

            return Math.Min(score, 100m);
        }

        // Reconciliation Process
        public async Task<BankReconciliation> StartReconciliationAsync(Guid bankAccountId, Guid statementId)
        {
            var bankAccount = await _context.BankAccounts.FindAsync(bankAccountId);
            var statement = await _context.BankStatements
                .Include(s => s.Items)
                .FirstOrDefaultAsync(s => s.Id == statementId);

            if (bankAccount == null || statement == null)
                throw new ArgumentException("Bank account or statement not found");

            var reconciliation = new BankReconciliation
            {
                BankAccountId = bankAccountId,
                BankStatementId = statementId,
                ReconciliationDate = DateTime.UtcNow,
                BookBalance = await CalculateBookBalanceAsync(bankAccountId),
                BankBalance = statement.ClosingBalance,
                Status = "InProgress"
            };

            _context.BankReconciliations.Add(reconciliation);
            await _context.SaveChangesAsync();

            return reconciliation;
        }

        public async Task<BankReconciliation> CompleteReconciliationAsync(Guid reconciliationId, string notes)
        {
            var reconciliation = await _context.BankReconciliations.FindAsync(reconciliationId);
            if (reconciliation == null)
                throw new ArgumentException("Reconciliation not found");

            reconciliation.Status = "Reconciled";
            reconciliation.ReconciledAt = DateTime.UtcNow;
            reconciliation.Notes = notes;

            await _context.SaveChangesAsync();
            return reconciliation;
        }

        // Helper Methods
        private string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            var current = "";
            var inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                var c = line[i];
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.Trim());
                    current = "";
                }
                else
                {
                    current += c;
                }
            }
            result.Add(current.Trim());
            return result.ToArray();
        }

        private decimal ParseDecimal(string value, string decimalSeparator)
        {
            if (string.IsNullOrEmpty(value)) return 0;
            
            value = value.Replace(",", ""); // Remove thousand separators
            if (decimalSeparator == ",")
                value = value.Replace(",", ".");
            
            return decimal.TryParse(value, NumberStyles.Currency, CultureInfo.InvariantCulture, out var result) ? result : 0;
        }

        private string DetermineTransactionType(string amount, string creditIndicator, string debitIndicator)
        {
            if (amount.Contains(creditIndicator))
                return "Credit";
            if (amount.Contains(debitIndicator))
                return "Debit";
            
            return decimal.Parse(amount.Replace(",", "")) >= 0 ? "Credit" : "Debit";
        }

        private List<BankStatementItem> ParseQifContent(string content)
        {
            // QIF parsing implementation
            return new List<BankStatementItem>();
        }

        private List<BankStatementItem> ParseOfxContent(string content)
        {
            // OFX parsing implementation
            return new List<BankStatementItem>();
        }

        private decimal CalculateStringSimilarity(string str1, string str2)
        {
            if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
                return 0;

            var longer = str1.Length > str2.Length ? str1 : str2;
            var shorter = str1.Length > str2.Length ? str2 : str1;

            if (longer.Length == 0) return 1.0m;

            var distance = LevenshteinDistance(longer, shorter);
            return (longer.Length - distance) / (decimal)longer.Length;
        }

        private int LevenshteinDistance(string str1, string str2)
        {
            var matrix = new int[str1.Length + 1, str2.Length + 1];

            for (int i = 0; i <= str1.Length; i++)
                matrix[i, 0] = i;

            for (int j = 0; j <= str2.Length; j++)
                matrix[0, j] = j;

            for (int i = 1; i <= str1.Length; i++)
            {
                for (int j = 1; j <= str2.Length; j++)
                {
                    var cost = str1[i - 1] == str2[j - 1] ? 0 : 1;
                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }

            return matrix[str1.Length, str2.Length];
        }

        private string GenerateMatchReason(BankStatementItem item, Transaction transaction, decimal score)
        {
            var reasons = new List<string>();
            
            var transactionAmount = transaction.TotalDebits - transaction.TotalCredits;
            if (Math.Abs(item.Amount - transactionAmount) <= 0.01m)
                reasons.Add("Exact amount");
            if (item.TransactionDate.Date == transaction.Date.Date)
                reasons.Add("Same date");
            if (CalculateStringSimilarity(item.Description, transaction.Description) > 0.8m)
                reasons.Add("Similar description");

            return string.Join(", ", reasons);
        }

        private async Task<decimal> CalculateBookBalanceAsync(Guid bankAccountId)
        {
            var transactions = await _context.Transactions
                .Where(t => t.Details.Any(d => d.AccountId == bankAccountId))
                .ToListAsync();

            return transactions.Sum(t => t.TotalDebits - t.TotalCredits);
        }

        // Placeholder implementations for remaining methods
        public async Task UnmatchTransactionAsync(Guid statementItemId) { await Task.CompletedTask; }
        public async Task<List<MatchingRule>> GetMatchingRulesAsync() { return new List<MatchingRule>(); }
        public async Task<MatchingRule> CreateMatchingRuleAsync(MatchingRule rule) { return rule; }
        public async Task<BankReconciliation> AddReconciliationItemAsync(Guid reconciliationId, ReconciliationItem item) { return new BankReconciliation(); }
        public async Task<BankReconciliation> RemoveReconciliationItemAsync(Guid reconciliationId, Guid itemId) { return new BankReconciliation(); }
        public async Task<BankReconciliation> UpdateReconciliationItemAsync(Guid reconciliationId, Guid itemId, ReconciliationItem item) { return new BankReconciliation(); }
        public async Task<BankReconciliation> CalculateReconciliationBalanceAsync(Guid reconciliationId) { return new BankReconciliation(); }
        public async Task<BankReconciliation> GetReconciliationAsync(Guid reconciliationId) { return new BankReconciliation(); }
        public async Task<List<BankReconciliation>> GetReconciliationsAsync(Guid bankAccountId) { return new List<BankReconciliation>(); }
        public async Task<List<OutstandingItem>> GetOutstandingItemsAsync(Guid bankAccountId) { return new List<OutstandingItem>(); }
        public async Task<OutstandingItem> CreateOutstandingItemAsync(OutstandingItem item) { return item; }
        public async Task<OutstandingItem> UpdateOutstandingItemAsync(OutstandingItem item) { return item; }
        public async Task DeleteOutstandingItemAsync(Guid itemId) { await Task.CompletedTask; }
        public async Task<List<OutstandingItem>> GetStaleItemsAsync(Guid bankAccountId) { return new List<OutstandingItem>(); }
        public async Task MarkItemAsClearedAsync(Guid itemId, DateTime clearedDate) { await Task.CompletedTask; }
        public async Task<ReconciliationReport> GenerateReconciliationReportAsync(Guid reconciliationId, string reportType) { return new ReconciliationReport(); }
        public async Task<byte[]> ExportReconciliationReportAsync(Guid reconciliationId, string format) { return new byte[0]; }
        public async Task<List<ReconciliationReport>> GetReconciliationReportsAsync(Guid reconciliationId) { return new List<ReconciliationReport>(); }
        public async Task<List<ReconciliationAudit>> GetReconciliationAuditTrailAsync(Guid reconciliationId) { return new List<ReconciliationAudit>(); }
        public async Task<ReconciliationAudit> LogReconciliationActionAsync(Guid reconciliationId, string action, string description, string oldValue, string newValue) { return new ReconciliationAudit(); }
        public async Task<ValidationResult> ValidateReconciliationAsync(Guid reconciliationId) { return new ValidationResult(); }
        public async Task<bool> CanReconcileAccountAsync(Guid bankAccountId) { return true; }
        public async Task<List<string>> GetReconciliationWarningsAsync(Guid reconciliationId) { return new List<string>(); }
        public async Task<decimal> CalculateDiscrepancyAsync(Guid reconciliationId) { return 0; }
        public async Task<List<BankStatement>> BulkImportStatementsAsync(List<BulkImportRequest> requests) { return new List<BankStatement>(); }
        public async Task<List<BankReconciliation>> BulkStartReconciliationsAsync(List<Guid> bankAccountIds) { return new List<BankReconciliation>(); }
        public async Task<List<MatchResult>> BulkMatchTransactionsAsync(List<BulkMatchRequest> requests) { return new List<MatchResult>(); }
        public async Task<ImportTemplate> CreateImportTemplateAsync(ImportTemplate template) { return template; }
        public async Task<List<ImportTemplate>> GetImportTemplatesAsync() { return new List<ImportTemplate>(); }
        public async Task<ImportTemplate> GetImportTemplateAsync(Guid templateId) { return new ImportTemplate(); }
        public async Task<BankStatement> ProcessStatementAsync(Guid statementId) { return new BankStatement(); }
        public async Task<List<BankStatementItem>> CategorizeStatementItemsAsync(Guid statementId) { return new List<BankStatementItem>(); }
        public async Task<BankStatement> ValidateStatementAsync(Guid statementId) { return new BankStatement(); }
        public async Task<BankStatement> FinalizeStatementAsync(Guid statementId) { return new BankStatement(); }
    }
}
