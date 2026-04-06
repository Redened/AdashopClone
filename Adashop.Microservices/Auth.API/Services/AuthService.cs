using Adashop.Shared.Results;
using Auth.API.Data;
using Auth.API.DTOs;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Auth.API.Services;

public class AuthService : IAuthService
{
    private readonly AuthDbContext _DB;
    private readonly ISMTPService _SMTP;
    private readonly IJWTService _JWT;
    private readonly ILogger<AuthService> _LOG;

    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<ResetPasswordRequest> _resetPasswordValidator;
    private readonly IValidator<ChangePasswordRequest> _changePasswordValidator;

    private const int VERIFICATION_CODE_EXPIRY_MINUTES = 15;
    private const int REFRESH_TOKEN_EXPIRY_DAYS = 7;

    public AuthService(
        AuthDbContext DB,
        ISMTPService SMTP,
        IJWTService JWT,
        ILogger<AuthService> LOG,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator,
        IValidator<ResetPasswordRequest> resetPasswordValidator,
        IValidator<ChangePasswordRequest> changePasswordValidator )
    {
        _DB = DB;
        _SMTP = SMTP;
        _JWT = JWT;
        _LOG = LOG;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _resetPasswordValidator = resetPasswordValidator;
        _changePasswordValidator = changePasswordValidator;
    }


    public async Task<Result<string>> Register( RegisterRequest request )
    {
        var normalizedEmail = request.Email.ToLowerInvariant();

        var validationResult = await _registerValidator.ValidateAsync(request);
        if ( !validationResult.IsValid )
        {
            var validationErrors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return Result<string>.Error(400, "Validation failed", validationErrors);
        }

        if ( await _DB.Users.AnyAsync(u => u.Email == normalizedEmail) )
        {
            _LOG.LogWarning("Registration attempt with existing email: {Email}", normalizedEmail);
            return Result<string>.Error(400, "Email already registered");
        }


        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new Entities.User()
        {
            Email = normalizedEmail,
            PasswordHash = passwordHash,
            VerificationToken = GenerateSecureCode(),
            VerificationTokenExpiry = DateTime.UtcNow.AddMinutes(VERIFICATION_CODE_EXPIRY_MINUTES)
        };

        try
        {
            _DB.Users.Add(user);
            await _DB.SaveChangesAsync();
            _LOG.LogInformation("User registered successfully: {Email}", normalizedEmail);
        }
        catch ( Exception ex )
        {
            _LOG.LogError(ex, "Database error during registration for user: {Email}", normalizedEmail);
            return Result<string>.Error(500, "Registration failed. Please try again later.");
        }


        var emailSent = false;
        try
        {
            var emailBody = $@"
                <h2>Email Verification</h2>
                <p>Verification code: <strong>{user.VerificationToken}</strong></p>
                <p>This code expires in {VERIFICATION_CODE_EXPIRY_MINUTES} minutes.</p>
            ";

            await _SMTP.SendEmailAsync(user.Email, "Email Verification", emailBody);
            emailSent = true;
            _LOG.LogInformation("Verification email sent to: {Email}", user.Email);
        }
        catch ( Exception ex )
        {
            _LOG.LogError(ex, "SMTP error during registration for user: {Email}", user.Email);
        }


        return emailSent
            ? Result<string>.Success(201, "Registration successful. Check your email for the verification code.")
            : Result<string>.Success(201, "Registration successful. Failed to send verification email. Please contact support.");
    }

    public async Task<Result<TokenResponse>> EmailVerification( EmailVerificationRequest request )
    {
        var normalizedEmail = request.Email.ToLowerInvariant();
        var user = await _DB.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);

        if ( user == null )
        {
            _LOG.LogWarning("Email verification attempt for non-existent user: {Email}", normalizedEmail);
            return Result<TokenResponse>.Error(404, "User not found");
        }

        if ( user.IsVerified )
            return Result<TokenResponse>.Error(400, "Email already verified");

        if ( user.VerificationToken != request.Code )
        {
            _LOG.LogWarning("Invalid verification code for user: {Email}", user.Email);
            return Result<TokenResponse>.Error(400, "Invalid verification code");
        }

        if ( user.VerificationTokenExpiry < DateTime.UtcNow )
        {
            _LOG.LogWarning("Expired verification code for user: {Email}", user.Email);
            return Result<TokenResponse>.Error(400, "Verification code has expired. Please request a new one.");
        }


        user.IsVerified = true;
        user.VerificationToken = null;
        user.VerificationTokenExpiry = null;

        user.RefreshToken = GenerateRefreshToken();
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(REFRESH_TOKEN_EXPIRY_DAYS);

        user.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _DB.SaveChangesAsync();
            _LOG.LogInformation("Email verified for user: {Email}", user.Email);
        }
        catch ( Exception ex )
        {
            _LOG.LogError(ex, "Database error during email verification for user: {Email}", user.Email);
            return Result<TokenResponse>.Error(500, "Verification failed. Please try again later.");
        }

        var accessToken = _JWT.GenerateToken(
            userId: user.Id.ToString(),
            email: user.Email,
            roles: [user.Role.ToString()]
        );

        return Result<TokenResponse>.Success(200, new TokenResponse(accessToken, user.RefreshToken));
    }

    public async Task<Result<TokenResponse>> Login( LoginRequest request )
    {
        var normalizedEmail = request.Email.ToLowerInvariant();

        var validationResult = await _loginValidator.ValidateAsync(request);
        if ( !validationResult.IsValid )
        {
            var validationErrors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return Result<TokenResponse>.Error(400, "Validation failed", validationErrors);
        }


        var user = await _DB.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);

        if ( user == null )
        {
            BCrypt.Net.BCrypt.Verify(request.Password, "$2a$11$dummy.hash.to.prevent.timing.attack");
            _LOG.LogWarning("Login attempt with non-existent email: {Email}", normalizedEmail);
            return Result<TokenResponse>.Error(400, "Email or password is incorrect");
        }

        if ( !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash) )
        {
            _LOG.LogWarning("Failed login attempt for user: {Email}", user.Email);
            return Result<TokenResponse>.Error(400, "Email or password is incorrect");
        }


        if ( !user.IsVerified )
        {
            user.VerificationToken = GenerateSecureCode();
            user.VerificationTokenExpiry = DateTime.UtcNow.AddMinutes(VERIFICATION_CODE_EXPIRY_MINUTES);

            try
            {
                await _DB.SaveChangesAsync();
            }
            catch ( Exception ex )
            {
                _LOG.LogError(ex, "Database error during login for unverified user: {Email}", user.Email);
            }

            try
            {
                var emailBody = $@"
                    <h2>Email Verification Required</h2>
                    <p>Verification code: <strong>{user.VerificationToken}</strong></p>
                    <p>This code expires in {VERIFICATION_CODE_EXPIRY_MINUTES} minutes.</p>
                ";

                await _SMTP.SendEmailAsync(user.Email, "Email Verification", emailBody);
            }
            catch ( Exception ex )
            {
                _LOG.LogError(ex, "SMTP error during login for user: {Email}", user.Email);
            }

            return Result<TokenResponse>.Error(403, "Email not verified. A new verification code has been sent to your email.");
        }


        user.RefreshToken = GenerateRefreshToken();
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(REFRESH_TOKEN_EXPIRY_DAYS);

        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _DB.SaveChangesAsync();
            _LOG.LogInformation("User logged in: {Email}", user.Email);
        }
        catch ( Exception ex )
        {
            _LOG.LogError(ex, "Database error during login for user: {Email}", user.Email);
            return Result<TokenResponse>.Error(500, "Login failed. Please try again later.");
        }

        var accessToken = _JWT.GenerateToken(
            userId: user.Id.ToString(),
            email: user.Email,
            roles: [user.Role.ToString()]
        );

        return Result<TokenResponse>.Success(200, new TokenResponse(accessToken, user.RefreshToken));
    }

    public async Task<Result<TokenResponse>> RefreshToken( RefreshTokenRequest request )
    {
        var user = await _DB.Users.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

        if ( user == null )
        {
            _LOG.LogWarning("Invalid refresh token used");
            return Result<TokenResponse>.Error(401, "Invalid refresh token");
        }

        if ( user.RefreshTokenExpiry < DateTime.UtcNow )
        {
            _LOG.LogWarning("Expired refresh token for user: {Email}", user.Email);
            return Result<TokenResponse>.Error(401, "Refresh token has expired. Please login again.");
        }


        var accessToken = _JWT.GenerateToken(
            userId: user.Id.ToString(),
            email: user.Email,
            roles: [user.Role.ToString()]
        );

        user.RefreshToken = GenerateRefreshToken();
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(REFRESH_TOKEN_EXPIRY_DAYS);

        user.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _DB.SaveChangesAsync();
            _LOG.LogInformation("Token refreshed for user: {Email}", user.Email);
        }
        catch ( Exception ex )
        {
            _LOG.LogError(ex, "Database error during token refresh for user: {Email}", user.Email);
            return Result<TokenResponse>.Error(500, "Token refresh failed. Please try again later.");
        }

        return Result<TokenResponse>.Success(200, new TokenResponse(accessToken, user.RefreshToken));
    }

    public async Task<Result<bool>> Logout( string userId )
    {
        if ( !int.TryParse(userId, out var id) )
            return Result<bool>.Error(400, "Invalid user ID");


        var user = await _DB.Users.FindAsync(id);

        if ( user == null )
            return Result<bool>.Error(404, "User not found");


        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;

        user.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _DB.SaveChangesAsync();
            _LOG.LogInformation("User logged out: {Email}", user.Email);
        }
        catch ( Exception ex )
        {
            _LOG.LogError(ex, "Database error during logout for user: {Email}", user.Email);
            return Result<bool>.Error(500, "Logout failed. Please try again later.");
        }

        return Result<bool>.Success(200, true);
    }

    public async Task<Result<string>> ForgotPassword( ForgotPasswordRequest request )
    {
        var normalizedEmail = request.Email.ToLowerInvariant();
        var user = await _DB.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);

        if ( user == null )
        {
            _LOG.LogWarning("Password reset requested for non-existent email: {Email}", normalizedEmail);
            return Result<string>.Success(200, "If the email exists, a password reset code has been sent.");
        }


        user.PasswordResetToken = GenerateSecureCode();
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddMinutes(15);

        try
        {
            await _DB.SaveChangesAsync();
        }
        catch ( Exception ex )
        {
            _LOG.LogError(ex, "Database error during password reset for user: {Email}", user.Email);
            return Result<string>.Error(500, "Failed to process password reset. Please try again later.");
        }

        try
        {
            var emailBody = $@"
                <h2>Password Reset</h2>
                <p>Password reset code: <strong>{user.PasswordResetToken}</strong></p>
                <p>This code expires in 15 minutes.</p>
                <p>If you didn't request this, please ignore this email.</p>
            ";

            await _SMTP.SendEmailAsync(user.Email, "Password Reset Code", emailBody);
            _LOG.LogInformation("Password reset email sent to: {Email}", user.Email);
        }
        catch ( Exception ex )
        {
            _LOG.LogError(ex, "SMTP error during password reset for user: {Email}", user.Email);
            return Result<string>.Error(500, "Failed to send password reset email. Please try again later.");
        }

        return Result<string>.Success(200, "If the email exists, a password reset code has been sent.");
    }

    public async Task<Result<TokenResponse>> ResetPassword( ResetPasswordRequest request )
    {
        var normalizedEmail = request.Email.ToLowerInvariant();

        var validationResult = await _resetPasswordValidator.ValidateAsync(request);
        if ( !validationResult.IsValid )
        {
            var validationErrors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return Result<TokenResponse>.Error(400, "Validation failed", validationErrors);
        }


        var user = await _DB.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);

        if ( user == null )
        {
            _LOG.LogWarning("Password reset attempt for non-existent user: {Email}", normalizedEmail);
            return Result<TokenResponse>.Error(404, "Invalid reset request");
        }

        if ( user.PasswordResetToken != request.Code )
        {
            _LOG.LogWarning("Invalid password reset code for user: {Email}", user.Email);
            return Result<TokenResponse>.Error(400, "Invalid reset code");
        }

        if ( user.PasswordResetTokenExpiry < DateTime.UtcNow )
        {
            _LOG.LogWarning("Expired password reset code for user: {Email}", user.Email);
            return Result<TokenResponse>.Error(400, "Reset code has expired. Please request a new one.");
        }


        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;

        user.RefreshToken = GenerateRefreshToken();
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        user.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _DB.SaveChangesAsync();
            _LOG.LogInformation("Password reset completed for user: {Email}", user.Email);
        }
        catch ( Exception ex )
        {
            _LOG.LogError(ex, "Database error during password reset for user: {Email}", user.Email);
            return Result<TokenResponse>.Error(500, "Password reset failed. Please try again later.");
        }

        var accessToken = _JWT.GenerateToken(
            userId: user.Id.ToString(),
            email: user.Email,
            roles: [user.Role.ToString()]
        );

        return Result<TokenResponse>.Success(200, new TokenResponse(accessToken, user.RefreshToken));
    }

    public async Task<Result<string>> ChangePassword( int userId, ChangePasswordRequest request )
    {
        var validationResult = await _changePasswordValidator.ValidateAsync(request);
        if ( !validationResult.IsValid )
        {
            var validationErrors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return Result<string>.Error(400, "Validation failed", validationErrors);
        }

        var user = await _DB.Users.FindAsync(userId);

        if ( user == null )
        {
            _LOG.LogWarning("Password change attempt for non-existent user ID: {UserId}", userId);
            return Result<string>.Error(404, "User not found");
        }

        if ( !BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash) )
        {
            _LOG.LogWarning("Failed password change attempt for user: {Email}", user.Email);
            return Result<string>.Error(400, "Current password is incorrect");
        }


        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        user.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _DB.SaveChangesAsync();
            _LOG.LogInformation("Password changed for user: {Email}", user.Email);
        }
        catch ( Exception ex )
        {
            _LOG.LogError(ex, "Database error during password change for user: {Email}", user.Email);
            return Result<string>.Error(500, "Password change failed. Please try again later.");
        }

        return Result<string>.Success(200, "Password changed successfully");
    }


    private static string GenerateSecureCode()
    {
        return RandomNumberGenerator.GetInt32(100_000, 999_999).ToString();
    }

    private static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }
}