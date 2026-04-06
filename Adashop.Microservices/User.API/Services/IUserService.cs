using Adashop.Shared.Results;
using User.API.DTOs;

namespace User.API.Services;

public interface IUserService
{
    Task<Result<UserDetailResponse>> ChangeUserDetails( int id, ChangeUserDetailsRequest request );
}