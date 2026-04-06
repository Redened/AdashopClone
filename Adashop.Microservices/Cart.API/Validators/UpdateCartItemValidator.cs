using Cart.API.DTOs;
using FluentValidation;

namespace Cart.API.Validators;

public class UpdateCartItemValidator : AbstractValidator<UpdateCartItemRequest>
{
    public UpdateCartItemValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero")
            .LessThanOrEqualTo(100).WithMessage("Quantity cannot exceed 100");
    }
}
