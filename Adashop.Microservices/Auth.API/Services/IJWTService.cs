namespace Auth.API.Services;

public interface IJWTService
{
    string GenerateToken( string userId, string email, List<string>? roles = null );
    bool ValidateToken( string token );
}