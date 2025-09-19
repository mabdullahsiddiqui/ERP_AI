using FluentValidation;

namespace ERP_AI.Data
{
    public class AccountValidator : AbstractValidator<Account>
    {
        public AccountValidator()
        {
            RuleFor(x => x.Code).NotEmpty().WithMessage("Account code is required.");
            RuleFor(x => x.Name).NotEmpty().WithMessage("Account name is required.");
        }
    }
}
