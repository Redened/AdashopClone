using Adashop.Common.Services.ExchangeRateAPI;
using Adashop.Common.Services.Helpers;
using Adashop.Common.Services.JWT;
using Adashop.Common.Services.SMTP;
using Adashop.Data;
using Adashop.Services.Admin;
using Adashop.Services.Auth;
using Adashop.Services.Background;
using Adashop.Services.Cart;
using Adashop.Services.Order;
using Adashop.Services.Product;
using FluentValidation;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace Adashop.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection ExtendServices( this IServiceCollection services, IConfiguration config )
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Enter {AccessToken}"
            });

            options.OperationFilter<SwaggerAuthorizeFilter>();

            var xmlFile = "Adashop.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if ( File.Exists(xmlPath) )
            {
                options.IncludeXmlComments(xmlPath);
            }
        });

        services.AddDbContext<DataContext>(options => options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        services.AddAuth(config);

        services.AddMemoryCache();

        services.AddValidatorsFromAssemblyContaining<Program>();

        services.AddHangfire(options => options.UseSqlServerStorage(config.GetConnectionString("DefaultConnection")));
        services.AddHangfireServer();

        services.AddHttpClient<ICurrencyService, CurrencyService>();

        services.AddScoped<IEmailJobService, EmailJobService>();
        services.AddScoped<ISMTPService, SMTPService>();
        services.AddScoped<IJWTService, JWTService>();

        services.AddScoped<ICategoryHelper, CategoryHelper>();
        services.AddScoped<ICurrencyHelper, CurrencyHelper>();
        services.AddScoped<IMapHelper, MapHelper>();

        services.AddScoped<IAuthServices, AuthServices>();
        services.AddScoped<IProductServices, ProductServices>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();

        services.AddScoped<IAdminServices, AdminServices>();


        return services;
    }
}

