using FluentValidation;

namespace ERP_AI.Data
{
    public class CustomerValidator : AbstractValidator<Customer>
    {
        public CustomerValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Customer name is required.")
                .Length(2, 100).WithMessage("Customer name must be between 2 and 100 characters.");

            RuleFor(x => x.Email)
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
                .WithMessage("Please enter a valid email address.");

            RuleFor(x => x.Phone)
                .Matches(@"^[\+]?[\d\s\-\(\)]{10,}$").When(x => !string.IsNullOrEmpty(x.Phone))
                .WithMessage("Please enter a valid phone number.");

            RuleFor(x => x.CreditLimit)
                .GreaterThanOrEqualTo(0).WithMessage("Credit limit cannot be negative.");

            RuleFor(x => x.CurrentBalance)
                .GreaterThanOrEqualTo(0).WithMessage("Current balance cannot be negative.");
        }
    }
}

