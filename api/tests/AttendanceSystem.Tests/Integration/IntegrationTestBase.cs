using System.Net.Http.Headers;
using AttendanceSystem.API.Data;
using AttendanceSystem.API.Models;
using AttendanceSystem.API.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AttendanceSystem.Tests.Integration;

public abstract class IntegrationTestBase : IClassFixture<TestWebApplicationFactory>, IDisposable
{
    protected readonly HttpClient Client;
    protected readonly TestWebApplicationFactory Factory;
    protected Guid TestOrganizationId;
    protected string TestApiKey = null!;

    protected IntegrationTestBase(TestWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        SeedTestData();
    }

    private void SeedTestData()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var apiKeyService = scope.ServiceProvider.GetRequiredService<ApiKeyService>();

        var org = new Organization
        {
            Id = Guid.NewGuid(),
            Name = "Test Organization",
            Slug = $"test-org-{Guid.NewGuid():N}",
            CreatedAt = DateTime.UtcNow
        };
        db.Organizations.Add(org);

        var (rawKey, keyEntity) = apiKeyService.GenerateKey(org.Id, "Test Key");
        db.ApiKeys.Add(keyEntity);
        db.SaveChanges();

        TestOrganizationId = org.Id;
        TestApiKey = rawKey;

        Client.DefaultRequestHeaders.Add("X-Api-Key", TestApiKey);
    }

    public void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }
}
