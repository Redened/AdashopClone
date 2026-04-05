using Adashop.DTOs;
using FluentValidation;

namespace Adashop.Validators.Admin;

public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.Name)
            .Length(2, 100).WithMessage("Category name must be between 2 and 100 characters")
            .When(x => x.Name != null);

        RuleFor(x => x.ParentCategoryId)
            .GreaterThan(0).WithMessage("Invalid parent category")
            .When(x => x.ParentCategoryId.HasValue);
    }
}
