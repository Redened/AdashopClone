using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Auth.API.Services;

public class JWTService : IJWTService
{
    private readonly IConfiguration _CONFIGURATION;

    public JWTService( IConfiguration configuration ) => _CONFIGURATION = configuration;


    public string GenerateToken( string userId, string email, List<string>? roles = null )
    {
        var secret = _CONFIGURATION["JWT:Secret"] ?? throw new InvalidOperationException("JWT:Secret is not configured");
        var issuer = _CONFIGURATION["JWT:Issuer"] ?? throw new InvalidOperationException("JWT:Issuer is not configured");
        var audience = _CONFIGURATION["JWT:Audience"] ?? throw new InvalidOperationException("JWT:Audience is not configured");
        if ( !int.TryParse(_CONFIGURATION["JWT:ExpirationMinutes"], out var expirationMinutes) )
            throw new InvalidOperationException("JWT:ExpirationMinutes is not configured or invalid");


        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, email),
            new(ClaimTypes.NameIdentifier, userId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };


        if ( roles != null )
        {
            foreach ( var role in roles )
                claims.Add(new Claim(ClaimTypes.Role, role));
        }


        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            issuer: issuer,
            audience: audience,
            signingCredentials: credentials,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes)
        );


        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool ValidateToken( string token )
    {
        var secret = _CONFIGURATION["JWT:Secret"] ?? throw new InvalidOperationException("JWT:Secret is not configured");
        var issuer = _CONFIGURATION["JWT:Issuer"] ?? throw new InvalidOperationException("JWT:Issuer is not configured");
        var audience = _CONFIGURATION["JWT:Audience"] ?? throw new InvalidOperationException("JWT:Audience is not configured");


        var key = Encoding.UTF8.GetBytes(secret);
        var tokenHandler = new JwtSecurityTokenHandler();


        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            }, out _);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
