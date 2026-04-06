using Auth.API.DTOs;
using FluentValidation;

namespace Auth.API.Validators;

public class ResetPasswordValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email format is invalid.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Reset code is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(10).WithMessage("Password must be at least 10 characters long.")
            .MaximumLength(50).WithMessage("Password must not exceed 50 characters.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number.")
            .Matches(@"[\W]").WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm Password is required.")
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match.");
    }
}