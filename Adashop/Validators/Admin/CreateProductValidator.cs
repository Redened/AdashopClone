using Adashop.DTOs;
using FluentValidation;

namespace Adashop.Validators.Admin;

public class CreateProductValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .Length(3, 200).WithMessage("Product name must be between 3 and 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters")
            .When(x => x.Description != null);

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Valid category is required");
    }
}