using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using MongoDB.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero,
            NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddMassTransit(x =>
{

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("bids", false));

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"] ?? "localhost", "/", h =>
        {
            h.Username(builder.Configuration.GetValue("RabbitMq:Username", "guest")!);
            h.Password(builder.Configuration.GetValue("RabbitMq:Password", "guest")!);
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await DB.InitAsync("BidDb", MongoClientSettings.FromConnectionString(
    builder.Configuration.GetConnectionString("BidDbConnection")
));

app.Run();
