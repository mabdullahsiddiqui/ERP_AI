using ERP_AI.Data;
using ERP_AI.Core;
using FluentValidation;

namespace ERP_AI.Desktop.ViewModels
{
    public class AccountEditViewModel : BaseViewModel
    {
        public Account Account { get; set; } = new();
        public AccountValidator Validator { get; } = new();
        public string ValidationError { get; set; } = string.Empty;

        public bool Validate()
        {
            var result = Validator.Validate(Account);
            ValidationError = result.IsValid ? string.Empty : string.Join("; ", result.Errors.Select(e => e.ErrorMessage));
            return result.IsValid;
        }

        public void AutoGenerateCode(IEnumerable<Account> existingAccounts)
        {
            // Example: Find max code in type, increment
            var maxCode = existingAccounts
                .Where(a => a.Type == Account.Type)
                .Select(a => int.TryParse(a.Code, out var c) ? c : 0)
                .DefaultIfEmpty(1000)
                .Max();
            Account.Code = (maxCode + 1).ToString();
        }
    }
}
