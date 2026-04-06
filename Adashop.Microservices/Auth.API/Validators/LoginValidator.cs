using Auth.API.DTOs;
using FluentValidation;

namespace Auth.API.Validators;

public class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email format is invalid.");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
