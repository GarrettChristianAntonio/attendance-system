using AttendanceSystem.API.Services;

namespace AttendanceSystem.API.Middleware;

public class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;

    public ApiKeyAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApiKeyService apiKeyService, IOrganizationContext orgContext)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

        // Skip auth for non-API routes, swagger, health check, and organization creation
        if (!path.StartsWith("/api/") || path.StartsWith("/api/organizations/create") ||
            path.Contains("swagger") || path == "/healthz")
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("X-Api-Key", out var apiKeyHeader))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "API key is required. Include X-Api-Key header." });
            return;
        }

        var rawKey = apiKeyHeader.ToString();
        var result = await apiKeyService.ValidateKeyAsync(rawKey);

        if (result is null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid or inactive API key." });
            return;
        }

        orgContext.OrganizationId = result.Value.organizationId;
        orgContext.OrganizationName = result.Value.orgName;

        await _next(context);
    }
}
