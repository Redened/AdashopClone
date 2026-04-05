using Adashop.Common.Results;
using Adashop.DTOs;

namespace Adashop.Services.Auth;

public interface IAuthServices
{
    Task<Result<string>> Register( RegisterRequest request );
    Task<Result<TokenResponse>> EmailVerification( EmailVerificationRequest request );

    Task<Result<TokenResponse>> Login( LoginRequest request );
    Task<Result<TokenResponse>> RefreshToken( RefreshTokenRequest request );
    Task<Result<bool>> Logout( string userId );

    Task<Result<TokenResponse>> ResetPassword( ResetPasswordRequest request );
    Task<Result<string>> ForgotPassword( ForgotPasswordRequest request );
    Task<Result<string>> ChangePassword( int userId, ChangePasswordRequest request );

}