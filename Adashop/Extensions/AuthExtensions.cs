using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Adashop.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddAuth( this IServiceCollection services, IConfiguration config )
    {
        var configJWTSecret = config["JWT:Secret"] ?? throw new Exception("JWT:Secret is missing in configuration");
        var configJWTIssuer = config["JWT:Issuer"] ?? throw new Exception("JWT:Issuer is missing in configuration");
        var configJWTAudience = config["JWT:Audience"] ?? throw new Exception("JWT:Audience is missing in configuration");


        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero,
                ValidIssuer = configJWTIssuer,
                ValidAudience = configJWTAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configJWTSecret))
            };
        });

        services.AddAuthorization();


        return services;
    }
}