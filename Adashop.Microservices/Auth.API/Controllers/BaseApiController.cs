using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Auth.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BaseApiController : ControllerBase
{
    protected int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if ( string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId) )
            throw new UnauthorizedAccessException("Invalid token");

        return userId;
    }
}