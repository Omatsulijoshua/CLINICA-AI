using System;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using ClinicaAI.Infrastructure.Data;
using ClinicaAI.Infrastructure.Security;
using ClinicaAI.Infrastructure.Storage;
using ClinicaAI.Infrastructure.Cache;
using ClinicaAI.Infrastructure.Agents;
using ClinicaAI.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Database Context Setup (PostgreSQL)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? 
                       "Host=localhost;Database=clinica_db;Username=clinica_user;Password=SecurePassword123!";
builder.Services.AddDbContext<ClinicaDbContext>(options =>
    options.UseNpgsql(connectionString));

// Caching and Storage Setup
builder.Services.AddSingleton<RedisCacheService>();
builder.Services.AddSingleton<MinIoStorageService>();
builder.Services.AddSingleton<JwtTokenService>();

// Register Medical Agents
builder.Services.AddScoped<MemoryAgent>();
builder.Services.AddScoped<EmergencyAgent>();
builder.Services.AddScoped<SymptomAgent>();
builder.Services.AddScoped<LabAgent>();
builder.Services.AddScoped<DrugAgent>();
builder.Services.AddScoped<NutritionAgent>();
builder.Services.AddScoped<ResearchAgent>();
builder.Services.AddScoped<VideoAgent>();
builder.Services.AddScoped<CoordinatorAgent>();

// JWT Authentication Setup
var jwtKey = builder.Configuration["Jwt:Key"] ?? "SuperSecretClinicaAIKey1234567890EncryptionKey!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "ClinicaAI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "ClinicaAIClients";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// Configure Rate Limiting Middleware
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://127.0.0.1:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Auto-Apply Migrations and Seed Databases on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ClinicaDbContext>();
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[DB Seeder Failure] Seed error occurred: {ex.Message}");
    }
}

app.UseCors();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// Custom HIPAA Audit Middleware
app.UseMiddleware<HipaaAuditMiddleware>();

app.MapControllers();

app.Run();

// Required to make integration tests compile smoothly by referencing program structure
public partial class Program { }
