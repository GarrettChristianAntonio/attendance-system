using System.Threading.RateLimiting;
using AttendanceSystem.API.Data;
using AttendanceSystem.API.Middleware;
using AttendanceSystem.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<FaceMatchingService>();
builder.Services.AddScoped<ApiKeyService>();
builder.Services.AddScoped<IOrganizationContext, OrganizationContext>();
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

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseCors("AllowFrontend");

app.UseMiddleware<ApiKeyAuthMiddleware>();
app.UseRateLimiter();

var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
Directory.CreateDirectory(uploadsPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.MapControllers();

app.Run();
