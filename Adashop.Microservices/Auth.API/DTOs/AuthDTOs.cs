namespace Auth.API.DTOs;

public record RegisterRequest( string Email, string Password, string ConfirmPassword );
public record LoginRequest( string Email, string Password );
public record EmailVerificationRequest( string Email, string Code );

public record RefreshTokenRequest( string RefreshToken );
public record TokenResponse( string AccessToken, string? RefreshToken = null );

public record ForgotPasswordRequest( string Email );
public record ResetPasswordRequest( string Email, string Code, string NewPassword, string ConfirmPassword );
public record ChangePasswordRequest( string CurrentPassword, string NewPassword, string ConfirmPassword );