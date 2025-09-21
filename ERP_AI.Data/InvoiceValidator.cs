using FluentValidation;

namespace ERP_AI.Data
{
    public class InvoiceValidator : AbstractValidator<Invoice>
    {
        public InvoiceValidator()
        {
            RuleFor(x => x.CustomerId)
                .NotEmpty().WithMessage("Customer is required.");

            RuleFor(x => x.InvoiceNumber)
                .NotEmpty().WithMessage("Invoice number is required.")
                .Length(3, 50).WithMessage("Invoice number must be between 3 and 50 characters.");

            RuleFor(x => x.InvoiceDate)
                .NotEmpty().WithMessage("Invoice date is required.")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Invoice date cannot be in the future.");

            RuleFor(x => x.DueDate)
                .NotEmpty().WithMessage("Due date is required.")
                .GreaterThanOrEqualTo(x => x.InvoiceDate).WithMessage("Due date must be after invoice date.");

            RuleFor(x => x.Subtotal)
                .GreaterThanOrEqualTo(0).WithMessage("Subtotal cannot be negative.");

            RuleFor(x => x.TaxAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Tax amount cannot be negative.");

            RuleFor(x => x.Total)
                .GreaterThanOrEqualTo(0).WithMessage("Total cannot be negative.");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required.")
                .Must(x => new[] { "Draft", "Sent", "Paid", "Overdue", "Void" }.Contains(x))
                .WithMessage("Invalid status value.");

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("Invoice must have at least one item.");
        }
    }
}

