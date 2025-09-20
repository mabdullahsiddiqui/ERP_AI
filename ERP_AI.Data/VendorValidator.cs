using FluentValidation;

namespace ERP_AI.Data
{
    public class VendorValidator : AbstractValidator<Vendor>
    {
        public VendorValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Vendor name is required.")
                .Length(2, 100).WithMessage("Vendor name must be between 2 and 100 characters.");

            RuleFor(x => x.Email)
                .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
                .WithMessage("Please enter a valid email address.");

            RuleFor(x => x.Phone)
                .Matches(@"^[\+]?[\d\s\-\(\)]{10,}$").When(x => !string.IsNullOrEmpty(x.Phone))
                .WithMessage("Please enter a valid phone number.");

            RuleFor(x => x.TaxId)
                .Length(9, 15).When(x => !string.IsNullOrEmpty(x.TaxId))
                .WithMessage("Tax ID must be between 9 and 15 characters.");
        }
    }
}

