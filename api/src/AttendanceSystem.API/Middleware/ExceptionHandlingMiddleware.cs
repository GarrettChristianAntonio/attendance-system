using System.Text.Json;

namespace AttendanceSystem.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);

            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/problem+json";

            var problem = new
            {
                type = "https://tools.ietf.org/html/rfc7807",
                title = "Internal Server Error",
                status = 500,
                detail = "An unexpected error occurred. Please try again later.",
                traceId = context.TraceIdentifier
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
    }
}
