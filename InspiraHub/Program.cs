using InspiraHub.Logging;
using InspiraHub.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using InspiraHub.Service;
using System.Security.Claims;


var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

DotNetEnv.Env.Load();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(jwtOptions =>
{
    jwtOptions.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
        ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY"))),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", p => 
        p.RequireClaim(ClaimTypes.Role, "Admin"));
    options.AddPolicy("RegularUser", p =>
        p.RequireClaim(ClaimTypes.Role, "RegularUser"));
});

builder.Services.AddControllers(option =>
{
    option.ReturnHttpNotAcceptable = true;
}).AddNewtonsoftJson();


builder.Services.AddDbContext<InspirahubContext>(options =>
    options.UseNpgsql($"Host={Environment.GetEnvironmentVariable("DATABASE_HOST")};Port={Environment.GetEnvironmentVariable("DATABASE_PORT")};Database={Environment.GetEnvironmentVariable("DATABASE_NAME")};Username={Environment.GetEnvironmentVariable("DATABASE_USERNAME")};Password={Environment.GetEnvironmentVariable("DATABASE_PASSWORD")}"));
builder.Services.AddSingleton<ILogging, Logging>();

builder.Services.AddSingleton<IHostedService, SchedulerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();