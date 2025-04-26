using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AttendanceSystem.API.Data;
using AttendanceSystem.API.Models;
using AttendanceSystem.API.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AttendanceSystem.Tests.Integration;

public class OrganizationIsolationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public OrganizationIsolationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private (HttpClient client, Guid orgId) CreateOrgClient(string orgName, string slug)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var apiKeyService = scope.ServiceProvider.GetRequiredService<ApiKeyService>();

        var org = new Organization
        {
            Id = Guid.NewGuid(),
            Name = orgName,
            Slug = slug,
            CreatedAt = DateTime.UtcNow
        };
        db.Organizations.Add(org);

        var (rawKey, keyEntity) = apiKeyService.GenerateKey(org.Id, $"{orgName} Key");
        db.ApiKeys.Add(keyEntity);

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            OrganizationId = org.Id,
            Name = $"{orgName} Employee",
            PhotoPath = "/test.jpg",
            FaceDescriptor = "[]",
            CreatedAt = DateTime.UtcNow
        };
        db.Employees.Add(employee);
        db.SaveChanges();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", rawKey);
        return (client, org.Id);
    }

    [Fact]
    public async Task OrgA_CannotSee_OrgB_Employees()
    {
        var (clientA, _) = CreateOrgClient("Org A", $"org-a-{Guid.NewGuid():N}");
        var (clientB, _) = CreateOrgClient("Org B", $"org-b-{Guid.NewGuid():N}");

        var responseA = await clientA.GetAsync("/api/employees");
        var responseB = await clientB.GetAsync("/api/employees");

        responseA.StatusCode.Should().Be(HttpStatusCode.OK);
        responseB.StatusCode.Should().Be(HttpStatusCode.OK);

        var employeesA = await responseA.Content.ReadFromJsonAsync<JsonElement[]>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var employeesB = await responseB.Content.ReadFromJsonAsync<JsonElement[]>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        employeesA.Should().NotBeNull();
        employeesB.Should().NotBeNull();

        // Each org should only see their own employees
        employeesA!.Should().AllSatisfy(e =>
            e.GetProperty("name").GetString().Should().Contain("Org A"));
        employeesB!.Should().AllSatisfy(e =>
            e.GetProperty("name").GetString().Should().Contain("Org B"));

        clientA.Dispose();
        clientB.Dispose();
    }

    [Fact]
    public async Task OrgA_CannotAccess_OrgB_EmployeeById()
    {
        var (clientA, _) = CreateOrgClient("Iso A", $"iso-a-{Guid.NewGuid():N}");
        var (clientB, orgBId) = CreateOrgClient("Iso B", $"iso-b-{Guid.NewGuid():N}");

        // Get org B's employee ID
        var responseB = await clientB.GetAsync("/api/employees");
        var employeesB = await responseB.Content.ReadFromJsonAsync<JsonElement[]>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var orgBEmployeeId = employeesB![0].GetProperty("id").GetString();

        // Try to access org B's employee with org A's key
        var crossResponse = await clientA.GetAsync($"/api/employees/{orgBEmployeeId}");

        crossResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        clientA.Dispose();
        clientB.Dispose();
    }
}
