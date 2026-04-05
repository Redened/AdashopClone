using Adashop.DTOs;
using FluentValidation;

namespace Adashop.Validators.Cart;

public class UpdateCartItemValidator : AbstractValidator<UpdateCartItemRequest>
{
    public UpdateCartItemValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero")
            .LessThanOrEqualTo(100).WithMessage("Quantity cannot exceed 100");
    }
}
