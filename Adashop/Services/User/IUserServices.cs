using Adashop.Common.Results;
using Adashop.DTOs;

namespace Adashop.Services.User;

public interface IUserServices
{
    Task<Result<UserDetailResponse>> ChangeUserDetails( int id, ChangeUserDetailsRequest request );
}