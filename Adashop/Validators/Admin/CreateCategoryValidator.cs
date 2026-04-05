using Adashop.DTOs;
using FluentValidation;

namespace Adashop.Validators.Admin;

public class CreateCategoryValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required")
            .Length(2, 100).WithMessage("Category name must be between 2 and 100 characters");

        RuleFor(x => x.ParentCategoryId)
            .GreaterThan(0).WithMessage("Invalid parent category")
            .When(x => x.ParentCategoryId.HasValue);
    }
}