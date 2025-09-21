using FluentValidation;

namespace ERP_AI.Data
{
    public class TransactionValidator : AbstractValidator<Transaction>
    {
        public TransactionValidator()
        {
            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("Transaction date is required.")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Transaction date cannot be in the future.");

            RuleFor(x => x.Reference)
                .Length(0, 100).WithMessage("Reference must be less than 100 characters.");

            RuleFor(x => x.Description)
                .Length(0, 500).WithMessage("Description must be less than 500 characters.");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required.")
                .Must(x => new[] { "Draft", "Posted", "Void" }.Contains(x))
                .WithMessage("Invalid status value.");

            RuleFor(x => x.TotalDebits)
                .GreaterThanOrEqualTo(0).WithMessage("Total debits cannot be negative.");

            RuleFor(x => x.TotalCredits)
                .GreaterThanOrEqualTo(0).WithMessage("Total credits cannot be negative.");

            RuleFor(x => x)
                .Must(x => x.IsBalanced).WithMessage("Transaction must be balanced (debits = credits).")
                .When(x => x.Status == "Posted");

            RuleFor(x => x.Details)
                .NotEmpty().WithMessage("Transaction must have at least one detail.")
                .Must(x => x.Count >= 2).WithMessage("Transaction must have at least 2 details for double-entry.")
                .When(x => x.Status == "Posted");
        }
    }
}

