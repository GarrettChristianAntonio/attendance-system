using AttendanceSystem.API.Services;

namespace AttendanceSystem.API.Middleware;

public class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;

    private static readonly string[] EmbedAllowedPrefixes =
    [
        "/api/widget/",
        "/api/attendance/today",
        "/api/attendance/live",
        "/api/face/match"
    ];

    public ApiKeyAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApiKeyService apiKeyService,
        EmbedTokenService embedTokenService, IOrganizationContext orgContext)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

        // Skip auth for non-API routes, swagger, health check, and organization creation
        if (!path.StartsWith("/api/") || path.StartsWith("/api/organizations/create") ||
            path.Contains("swagger") || path == "/healthz")
        {
            await _next(context);
            return;
        }

        // 1. Check Bearer token (Authorization header)
        var authHeader = context.Request.Headers.Authorization.ToString();
        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var jwt = authHeader["Bearer ".Length..].Trim();
            if (await TryAuthenticateJwt(jwt, path, embedTokenService, orgContext, context))
                return;
        }

        // 2. Check query string token (?token=...)
        if (context.Request.Query.TryGetValue("token", out var queryToken) && queryToken.Count > 0)
        {
            var jwt = queryToken.ToString();
            if (await TryAuthenticateJwt(jwt, path, embedTokenService, orgContext, context))
                return;
        }

        // 3. Check X-Api-Key header
        if (context.Request.Headers.TryGetValue("X-Api-Key", out var apiKeyHeader))
        {
            var rawKey = apiKeyHeader.ToString();
            var result = await apiKeyService.ValidateKeyAsync(rawKey);

            if (result is not null)
            {
                orgContext.OrganizationId = result.Value.organizationId;
                orgContext.OrganizationName = result.Value.orgName;
                await _next(context);
                return;
            }
        }

        // No valid authentication found
        context.Response.StatusCode = 401;
        await context.Response.WriteAsJsonAsync(new { error = "Authentication required. Provide X-Api-Key header, Bearer token, or ?token= query parameter." });
    }

    private async Task<bool> TryAuthenticateJwt(string jwt, string path,
        EmbedTokenService embedTokenService, IOrganizationContext orgContext, HttpContext context)
    {
        var result = embedTokenService.ValidateToken(jwt);
        if (result is null) return false;

        var (orgId, scopes) = result.Value;

        // Embed tokens can only access widget/embed endpoints
        var isEmbedPath = EmbedAllowedPrefixes.Any(prefix => path.StartsWith(prefix));
        if (!isEmbedPath)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { error = "Embed tokens can only access widget endpoints." });
            return true; // handled, don't continue to other auth methods
        }

        orgContext.OrganizationId = orgId;
        orgContext.OrganizationName = "Embed";
        await _next(context);
        return true;
    }
}
