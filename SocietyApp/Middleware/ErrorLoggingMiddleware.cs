using SocietyApp.Data;
using SocietyApp.Models;

namespace SocietyApp.Middleware;

public class ErrorLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorLoggingMiddleware> _logger;

    public ErrorLoggingMiddleware(RequestDelegate next, ILogger<ErrorLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var requestId = context.TraceIdentifier;

            try
            {
                var log = new ErrorLog
                {
                    RequestId = requestId,
                    Path = context.Request.Path,
                    Method = context.Request.Method,
                    ErrorMessage = ex.Message,
                    StackTrace = ex.StackTrace,
                    UserId = context.User?.Identity?.IsAuthenticated == true
                        ? context.User.FindFirst("sub")?.Value ?? context.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
                        : null,
                    OccurredAtUtc = DateTime.UtcNow
                };

                dbContext.ErrorLogs.Add(log);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception logEx)
            {
                // Logging to DB should never prevent the friendly error page.
                _logger.LogError(logEx, "Failed to persist error log for RequestId {RequestId}", requestId);
            }

            _logger.LogError(ex, "Unhandled exception for RequestId {RequestId}", requestId);
            throw;
        }
    }
}
