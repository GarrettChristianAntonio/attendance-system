using AttendanceSystem.API.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AttendanceSystem.Tests.Integration;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"TestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove all EF Core related services to avoid dual provider conflict
            var descriptorsToRemove = services
                .Where(d => d.ServiceType.FullName?.Contains("EntityFrameworkCore") == true
                         || d.ServiceType == typeof(DbContextOptions<AppDbContext>)
                         || d.ServiceType == typeof(DbContextOptions)
                         || d.ImplementationType?.FullName?.Contains("EntityFrameworkCore") == true)
                .ToList();
            foreach (var descriptor in descriptorsToRemove)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));
        });

        builder.UseEnvironment("Testing");
    }
}
