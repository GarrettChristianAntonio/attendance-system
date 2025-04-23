using System.Threading.RateLimiting;
using AttendanceSystem.API.Data;
using AttendanceSystem.API.Middleware;
using AttendanceSystem.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<FaceMatchingService>();
builder.Services.AddScoped<ApiKeyService>();
builder.Services.AddScoped<IOrganizationContext, OrganizationContext>();
builder.Services.AddScoped<PunctualityService>();
builder.Services.AddScoped<CheckOutService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddHostedService<AbsenceDetectionService>();
builder.Services.AddSingleton<WebhookService>();
builder.Services.AddSingleton<AttendanceLiveService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<WebhookService>());
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddPolicy("api", context =>
    {
        var apiKey = context.Request.Headers["X-Api-Key"].ToString();
        return RateLimitPartition.GetFixedWindowLimiter(
            apiKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("AllowFrontend");

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Attendance System API v1");
    options.RoutePrefix = "swagger";
});

app.UseMiddleware<ApiKeyAuthMiddleware>();
app.UseRateLimiter();

var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
Directory.CreateDirectory(uploadsPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.MapGet("/healthz", async (AppDbContext db) =>
{
    try
    {
        await db.Database.CanConnectAsync();
        return Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
    catch
    {
        return Results.StatusCode(503);
    }
});

app.MapControllers();

app.Run();
