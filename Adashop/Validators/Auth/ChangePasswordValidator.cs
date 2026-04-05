using Adashop.DTOs;
using FluentValidation;

namespace Adashop.Validators.Auth;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required");

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