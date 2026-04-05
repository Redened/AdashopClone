using Adashop.DTOs;
using FluentValidation;

namespace Adashop.Validators.Admin;

public class UpdateProductValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Name)
            .Length(3, 200).WithMessage("Product name must be between 3 and 200 characters")
            .When(x => x.Name != null);

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters")
            .When(x => x.Description != null);

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero")
            .When(x => x.Price.HasValue);

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative")
            .When(x => x.Stock.HasValue);

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Valid category is required")
            .When(x => x.CategoryId.HasValue);
    }
}