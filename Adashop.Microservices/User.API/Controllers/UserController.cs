using Adashop.Shared.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using User.API.DTOs;
using User.API.Services;

namespace User.API.Controllers;

[Authorize]
[ApiController]
[Route("api/")]
public class UserController : BaseApiController
{
    private readonly IUserService _userService;

    public UserController( IUserService userService ) => _userService = userService;

    /// <summary>
    /// Updates user details (first name, last name, phone number, and address).
    /// </summary>
    /// <param name="id">The user ID to update</param>
    /// <param name="request">ChangeUserDetailsRequest containing updated user details</param>
    /// <returns>UserDetailResponse containing the updated user information</returns>
    [HttpPut("users/{id}/details")]
    public async Task<IActionResult> ChangeUserDetails( int id, ChangeUserDetailsRequest request )
    {
        var result = await _userService.ChangeUserDetails(id, request);
        return StatusCode(result.Status, result);
    }
}
