using FluentValidation;

namespace ERP_AI.Data
{
    public class TransactionDetailValidator : AbstractValidator<TransactionDetail>
    {
        public TransactionDetailValidator()
        {
            RuleFor(x => x.TransactionId)
                .NotEmpty().WithMessage("Transaction ID is required.");

            RuleFor(x => x.AccountId)
                .NotEmpty().WithMessage("Account is required.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero.");

            RuleFor(x => x.Description)
                .Length(0, 200).WithMessage("Description must be less than 200 characters.");
        }
    }
}

