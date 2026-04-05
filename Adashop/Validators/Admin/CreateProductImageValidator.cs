using Adashop.DTOs;
using FluentValidation;

namespace Adashop.Validators.Admin;

public class CreateProductImageValidator : AbstractValidator<CreateProductImageRequest>
{
    public CreateProductImageValidator()
    {
        RuleFor(x => x.ImageUrl)
            .NotEmpty().WithMessage("Image URL is required")
            .Matches(@"^https?://").WithMessage("Image URL must start with http:// or https://");

        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Valid product is required");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Sort order cannot be negative");
    }
}
