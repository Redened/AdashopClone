using Adashop.DTOs;
using Adashop.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adashop.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : BaseApiController
{
    private readonly IAuthServices _authServices;

    public AuthController( IAuthServices authService ) => _authServices = authService;

    /// <summary>
    /// Registers a new user account with email and password.
    /// </summary>
    /// <param name="request">RegisterRequest containing email, password, password confirmation</param>
    /// <returns>AuthResponse containing access token and user information</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register( RegisterRequest request )
    {
        var result = await _authServices.Register(request);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Verifies a user's email address using the verification token sent to their email.
    /// </summary>
    /// <param name="request">EmailVerificationRequest containing email and verification token</param>
    /// <returns>Boolean indicating successful email verification</returns>
    [HttpPost("email-verification")]
    public async Task<IActionResult> EmailVerification( EmailVerificationRequest request )
    {
        var result = await _authServices.EmailVerification(request);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Authenticates a user and returns access and refresh tokens.
    /// </summary>
    /// <param name="request">LoginRequest containing email and password</param>
    /// <returns>AuthResponse containing access token and user information</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login( LoginRequest request )
    {
        var result = await _authServices.Login(request);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Refreshes an expired access token using a valid refresh token.
    /// </summary>
    /// <param name="request">RefreshTokenRequest containing the refresh token</param>
    /// <returns>AuthResponse containing new access token</returns>
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken( RefreshTokenRequest request )
    {
        var result = await _authServices.RefreshToken(request);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Logs out the current authenticated user and invalidates their tokens.
    /// </summary>
    /// <returns>Boolean indicating successful logout</returns>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = GetCurrentUserId();

        var result = await _authServices.Logout(userId.ToString());
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Initiates a password reset by sending a reset token to the user's email.
    /// </summary>
    /// <param name="request">ForgotPasswordRequest containing email address</param>
    /// <returns>Boolean indicating successful password reset initiation</returns>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword( ForgotPasswordRequest request )
    {
        var result = await _authServices.ForgotPassword(request);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Resets a user's password using a valid reset token.
    /// </summary>
    /// <param name="request">ResetPasswordRequest containing email, reset token, and new password</param>
    /// <returns>Boolean indicating successful password reset</returns>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword( ResetPasswordRequest request )
    {
        var result = await _authServices.ResetPassword(request);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Changes the password for the authenticated user.
    /// </summary>
    /// <param name="request">ChangePasswordRequest containing old and new passwords</param>
    /// <returns>Boolean indicating successful password change</returns>
    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword( ChangePasswordRequest request )
    {
        var userId = GetCurrentUserId();

        var result = await _authServices.ChangePassword(userId, request);
        return StatusCode(result.Status, result);
    }
}
