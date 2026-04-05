using Adashop.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ExtendServices(builder.Configuration);

var app = builder.Build();

app.ExtendApplication();

app.Run();