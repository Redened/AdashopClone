using Auth.API.Data;
using Auth.API.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Auth.API;

public class Program
{
    public static void Main( string[] args )
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<AuthDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IJWTService, JWTService>();
        builder.Services.AddScoped<ISMTPService, SMTPService>();

        builder.Services.AddValidatorsFromAssemblyContaining<Program>();

        var JWTSettings = builder.Configuration.GetSection("JWT");
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = JWTSettings["Issuer"],
                    ValidAudience = JWTSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JWTSettings["SecretKey"]!))
                };
            });

        builder.Services.AddAuthorization();

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        var app = builder.Build();

        if ( app.Environment.IsDevelopment() )
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowAll");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}
