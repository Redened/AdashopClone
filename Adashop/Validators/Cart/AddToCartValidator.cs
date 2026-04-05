using Adashop.DTOs;
using FluentValidation;

namespace Adashop.Validators.Cart;

public class AddToCartValidator : AbstractValidator<AddToCartRequest>
{
    public AddToCartValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Valid product is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero")
            .LessThanOrEqualTo(100).WithMessage("Quantity cannot exceed 100");
    }
}
