using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ClinicaAI.Core.Entities;
using ClinicaAI.Infrastructure.Data;

namespace ClinicaAI.Api.Middleware
{
    public class HipaaAuditMiddleware
    {
        private readonly RequestDelegate _next;

        public HipaaAuditMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? "";
            
            // Call next middleware
            await _next(context);

            // Audit logging trigger for clinical or authentication endpoints
            bool isClinicalRoute = path.Contains("/api/profile", StringComparison.OrdinalIgnoreCase) ||
                                   path.Contains("/api/chat", StringComparison.OrdinalIgnoreCase) ||
                                   path.Contains("/api/reports", StringComparison.OrdinalIgnoreCase) ||
                                   path.Contains("/api/auth", StringComparison.OrdinalIgnoreCase);

            if (isClinicalRoute)
            {
                try
                {
                    // Scope DB Context
                    using var scope = context.RequestServices.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<ClinicaDbContext>();

                    // Fetch user info
                    var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    Guid? userId = null;
                    if (Guid.TryParse(userIdClaim, out var parsedId))
                    {
                        userId = parsedId;
                    }

                    var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                    var action = $"{context.Request.Method} {path}";
                    var details = $"Status Code: {context.Response.StatusCode}. Query String: {context.Request.QueryString}";

                    var auditLog = new AuditLog
                    {
                        UserId = userId,
                        Action = action,
                        IpAddress = ipAddress,
                        Details = details,
                        Timestamp = DateTime.UtcNow
                    };

                    dbContext.AuditLogs.Add(auditLog);
                    await dbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // Fail-safes: Never crash user request if logging fails, but log to Console for diagnostic audit
                    Console.WriteLine($"[Audit System Failure] Logging failed: {ex.Message}");
                }
            }
        }
    }
}
