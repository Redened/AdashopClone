using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class SwaggerAuthorizeFilter : IOperationFilter
{
    public void Apply( OpenApiOperation operation, OperationFilterContext context )
    {
        var authorizeAttributes = context.MethodInfo.DeclaringType!
            .GetCustomAttributes(true)
            .OfType<AuthorizeAttribute>()
            .Concat(
                context.MethodInfo
                    .GetCustomAttributes(true)
                    .OfType<AuthorizeAttribute>()
            )
            .ToList();

        if ( !authorizeAttributes.Any() )
            return;

        operation.Security = new List<OpenApiSecurityRequirement>
        {
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Id = "Bearer",
                            Type = ReferenceType.SecurityScheme
                        }
                    },
                    Array.Empty<string>()
                }
            }
        };

        var roles = authorizeAttributes
            .Where(a => !string.IsNullOrEmpty(a.Roles))
            .SelectMany(a => a.Roles!.Split(','))
            .Select(r => r.Trim())
            .Distinct()
            .ToList();

        if ( roles.Any() )
        {
            operation.Description += $"\n\n🔐 **Requires Authentication with Role:** {string.Join(", ", roles)}";
        }
        else
        {
            operation.Description += "\n\n🔐 **Requires Authentication**";
        }
    }
}