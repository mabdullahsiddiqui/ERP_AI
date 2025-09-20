using FluentValidation;

namespace ERP_AI.Data
{
    public class BillValidator : AbstractValidator<Bill>
    {
        public BillValidator()
        {
            RuleFor(x => x.VendorId)
                .NotEmpty().WithMessage("Vendor is required.");

            RuleFor(x => x.BillNumber)
                .NotEmpty().WithMessage("Bill number is required.")
                .Length(3, 50).WithMessage("Bill number must be between 3 and 50 characters.");

            RuleFor(x => x.BillDate)
                .NotEmpty().WithMessage("Bill date is required.")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Bill date cannot be in the future.");

            RuleFor(x => x.DueDate)
                .NotEmpty().WithMessage("Due date is required.")
                .GreaterThanOrEqualTo(x => x.BillDate).WithMessage("Due date must be after bill date.");

            RuleFor(x => x.Subtotal)
                .GreaterThanOrEqualTo(0).WithMessage("Subtotal cannot be negative.");

            RuleFor(x => x.TaxAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Tax amount cannot be negative.");

            RuleFor(x => x.Total)
                .GreaterThanOrEqualTo(0).WithMessage("Total cannot be negative.");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required.")
                .Must(x => new[] { "Draft", "Received", "Paid", "Overdue", "Void" }.Contains(x))
                .WithMessage("Invalid status value.");

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("Bill must have at least one item.");
        }
    }
}

