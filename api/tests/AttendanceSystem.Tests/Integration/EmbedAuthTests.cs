using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AttendanceSystem.API.Data;
using AttendanceSystem.API.Models;
using AttendanceSystem.API.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AttendanceSystem.Tests.Integration;

public class EmbedAuthTests : IntegrationTestBase
{
    public EmbedAuthTests(TestWebApplicationFactory factory) : base(factory) { }

    private async Task<string> CreateEmbedToken(string[] scopes)
    {
        var response = await Client.PostAsJsonAsync("/api/embed/tokens", new
        {
            Name = "Test Embed",
            Scopes = scopes,
            ExpiresInDays = 30
        });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("token").GetString()!;
    }

    [Fact]
    public async Task EmbedToken_CanAccessWidgetEndpoint()
    {
        var token = await CreateEmbedToken(["embed:status"]);

        var embedClient = Factory.CreateClient();
        embedClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var response = await embedClient.GetAsync("/api/widget/status");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        embedClient.Dispose();
    }

    [Fact]
    public async Task EmbedToken_CannotAccessNonWidgetEndpoint()
    {
        var token = await CreateEmbedToken(["embed:status"]);

        var embedClient = Factory.CreateClient();
        embedClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        var response = await embedClient.GetAsync("/api/employees");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        embedClient.Dispose();
    }

    [Fact]
    public async Task EmbedToken_QueryParam_Works()
    {
        var token = await CreateEmbedToken(["embed:status"]);

        var embedClient = Factory.CreateClient();
        // No Authorization header — use query param instead
        var response = await embedClient.GetAsync($"/api/widget/status?token={token}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        embedClient.Dispose();
    }

    [Fact]
    public async Task EmbedToken_Expired_Returns401()
    {
        // Create an expired token directly using the service
        using var scope = Factory.Services.CreateScope();
        var embedService = scope.ServiceProvider.GetRequiredService<EmbedTokenService>();
        var (jwt, _) = await embedService.GenerateToken(TestOrganizationId, "Expired", ["embed:status"], -1);

        var embedClient = Factory.CreateClient();
        embedClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwt}");

        var response = await embedClient.GetAsync("/api/widget/status");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        embedClient.Dispose();
    }

    [Fact]
    public async Task CreateEmbedToken_ReturnsJwt()
    {
        var response = await Client.PostAsJsonAsync("/api/embed/tokens", new
        {
            Name = "Dashboard Widget",
            Scopes = new[] { "embed:checkin", "embed:status" },
            ExpiresInDays = 60
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("token").GetString().Should().NotBeNullOrEmpty();
        body.GetProperty("expiresAt").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ListEmbedTokens_ReturnsTokens()
    {
        await CreateEmbedToken(["embed:status"]);

        var response = await Client.GetAsync("/api/embed/tokens");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tokens = await response.Content.ReadFromJsonAsync<JsonElement[]>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        tokens.Should().NotBeNull();
        tokens!.Length.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task RevokeEmbedToken_ReturnsNoContent()
    {
        await CreateEmbedToken(["embed:status"]);

        var listResponse = await Client.GetAsync("/api/embed/tokens");
        var tokens = await listResponse.Content.ReadFromJsonAsync<JsonElement[]>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var tokenId = tokens![0].GetProperty("id").GetString();

        var deleteResponse = await Client.DeleteAsync($"/api/embed/tokens/{tokenId}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
