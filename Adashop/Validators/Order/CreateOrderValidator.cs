using Adashop.DTOs;
using FluentValidation;

namespace Adashop.Validators.Order;

public class CreateOrderValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.ShippingAddress)
            .NotEmpty().WithMessage("Shipping address is required")
            .Length(5, 100).WithMessage("Shipping address must be between 5 and 100 characters");
    }
}
