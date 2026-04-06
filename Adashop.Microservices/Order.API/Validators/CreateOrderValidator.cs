using FluentValidation;
using Order.API.DTOs;

namespace Order.API.Validators;

public class CreateOrderValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.ShippingAddress)
            .NotEmpty().WithMessage("Shipping address is required")
            .Length(5, 100).WithMessage("Shipping address must be between 5 and 100 characters");
    }
}
