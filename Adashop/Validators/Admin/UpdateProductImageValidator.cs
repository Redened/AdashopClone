using Adashop.DTOs;
using FluentValidation;

namespace Adashop.Validators.Admin;

public class UpdateProductImageValidator : AbstractValidator<UpdateProductImageRequest>
{
    public UpdateProductImageValidator()
    {
        RuleFor(x => x.ImageUrl)
            .Matches(@"^https?://").WithMessage("Image URL must start with http:// or https://")
            .When(x => x.ImageUrl != null);

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Sort order cannot be negative")
            .When(x => x.SortOrder.HasValue);
    }
}
