using Adashop.DTOs;
using FluentValidation;

namespace Adashop.Validators.User;

public class ChangeUserDetailsValidator : AbstractValidator<ChangeUserDetailsRequest>
{
    public ChangeUserDetailsValidator()
    {
        RuleFor(x => x.FirstName)
            .MaximumLength(100)
            .WithMessage("First name must not exceed 100 characters")
            .When(x => x.FirstName != null);

        RuleFor(x => x.LastName)
            .MaximumLength(100)
            .WithMessage("Last name must not exceed 100 characters")
            .When(x => x.LastName != null);

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .WithMessage("Phone number must be a valid format (E.164)")
            .When(x => x.PhoneNumber != null);

        RuleFor(x => x.Address)
            .MaximumLength(255)
            .WithMessage("Address must not exceed 255 characters")
            .When(x => x.Address != null);
    }
}
