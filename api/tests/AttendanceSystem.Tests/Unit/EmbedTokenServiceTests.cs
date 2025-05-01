using AttendanceSystem.API.Data;
using AttendanceSystem.API.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AttendanceSystem.Tests.Unit;

public class EmbedTokenServiceTests
{
    private readonly AppDbContext _db;
    private readonly EmbedTokenService _service;
    private readonly Guid _orgId = Guid.NewGuid();

    public EmbedTokenServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"EmbedTokenTests_{Guid.NewGuid()}")
            .Options;

        _db = new AppDbContext(options);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "test-secret-key-that-is-at-least-32-characters-long!"
            })
            .Build();

        _service = new EmbedTokenService(_db, config);
    }

    [Fact]
    public async Task GenerateToken_ReturnsValidJwt()
    {
        var scopes = new[] { "embed:checkin", "embed:status" };

        var (jwt, entity) = await _service.GenerateToken(_orgId, "Test Token", scopes, 30);

        jwt.Should().NotBeNullOrEmpty();
        jwt.Split('.').Should().HaveCount(3); // JWT has 3 parts
        entity.Name.Should().Be("Test Token");
        entity.Scopes.Should().Be("embed:checkin,embed:status");
        entity.OrganizationId.Should().Be(_orgId);
    }

    [Fact]
    public async Task ValidateToken_ValidJwt_ReturnsCorrectClaims()
    {
        var scopes = new[] { "embed:checkin" };
        var (jwt, _) = await _service.GenerateToken(_orgId, "Valid Token", scopes, 30);

        var result = _service.ValidateToken(jwt);

        result.Should().NotBeNull();
        result!.Value.organizationId.Should().Be(_orgId);
        result.Value.scopes.Should().Contain("embed:checkin");
    }

    [Fact]
    public async Task ValidateToken_ExpiredToken_ReturnsNull()
    {
        var (jwt, _) = await _service.GenerateToken(_orgId, "Expired Token", ["embed:status"], -1);

        var result = _service.ValidateToken(jwt);

        result.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_InvalidToken_ReturnsNull()
    {
        var result = _service.ValidateToken("not.a.valid.jwt.token");

        result.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_TamperedToken_ReturnsNull()
    {
        var result = _service.ValidateToken("eyJhbGciOiJIUzI1NiJ9.eyJvcmdfaWQiOiIxMjM0NSJ9.invalid_signature");

        result.Should().BeNull();
    }

    [Fact]
    public async Task RevokeToken_ExistingToken_ReturnsTrue()
    {
        var (_, entity) = await _service.GenerateToken(_orgId, "Revoke Me", ["embed:checkin"], 30);

        var revoked = await _service.RevokeToken(entity.Id, _orgId);

        revoked.Should().BeTrue();
        var tokens = await _service.GetTokensForOrg(_orgId);
        tokens.Should().NotContain(t => t.Id == entity.Id);
    }

    [Fact]
    public async Task RevokeToken_WrongOrg_ReturnsFalse()
    {
        var (_, entity) = await _service.GenerateToken(_orgId, "Wrong Org", ["embed:checkin"], 30);

        var revoked = await _service.RevokeToken(entity.Id, Guid.NewGuid());

        revoked.Should().BeFalse();
    }
}
