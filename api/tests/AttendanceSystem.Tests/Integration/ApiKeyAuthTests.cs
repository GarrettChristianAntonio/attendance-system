using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace AttendanceSystem.Tests.Integration;

public class ApiKeyAuthTests : IntegrationTestBase
{
    public ApiKeyAuthTests(TestWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task MissingApiKey_Returns401()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/employees");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        client.Dispose();
    }

    [Fact]
    public async Task InvalidApiKey_Returns401()
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", "ak_completely_invalid_key");

        var response = await client.GetAsync("/api/employees");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        client.Dispose();
    }

    [Fact]
    public async Task ValidApiKey_Returns200()
    {
        // Client already has a valid API key from IntegrationTestBase
        var response = await Client.GetAsync("/api/employees");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task OrganizationCreate_NoAuthRequired()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/organizations/create", new
        {
            Name = "No Auth Org",
            Slug = $"no-auth-{Guid.NewGuid():N}"
        });

        response.IsSuccessStatusCode.Should().BeTrue();
        client.Dispose();
    }

    [Fact]
    public async Task HealthCheck_NoAuthRequired()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/healthz");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        client.Dispose();
    }
}
