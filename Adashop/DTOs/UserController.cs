using Adashop.Controllers;
using Adashop.Services.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adashop.DTOs;

[Authorize]
[ApiController]
[Route("api/")]
public class UserController : BaseApiController
{
    private readonly IUserServices _userServices;

    public UserController( IUserServices userServices ) => _userServices = userServices;

    /// <summary>
    /// Updates user details (first name, last name, phone number, and address).
    /// </summary>
    /// <param name="id">The user ID to update</param>
    /// <param name="request">ChangeUserDetailsRequest containing updated user details</param>
    /// <returns>UserDetailResponse containing the updated user information</returns>
    [HttpPut("users/{id}/details")]
    public async Task<IActionResult> ChangeUserDetails( int id, ChangeUserDetailsRequest request )
    {
        var result = await _userServices.ChangeUserDetails(id, request);
        return StatusCode(result.Status, result);
    }
}
