using FluentValidation;

namespace ERP_AI.Data
{
    public class InvoiceItemValidator : AbstractValidator<InvoiceItem>
    {
        public InvoiceItemValidator()
        {
            RuleFor(x => x.InvoiceId)
                .NotEmpty().WithMessage("Invoice ID is required.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .Length(1, 200).WithMessage("Description must be between 1 and 200 characters.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

            RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price cannot be negative.");

            RuleFor(x => x.Amount)
                .GreaterThanOrEqualTo(0).WithMessage("Amount cannot be negative.");

            RuleFor(x => x.TaxRate)
                .InclusiveBetween(0, 100).WithMessage("Tax rate must be between 0 and 100 percent.");

            RuleFor(x => x.ItemCode)
                .Length(0, 50).WithMessage("Item code must be less than 50 characters.");
        }
    }
}

