using AttendanceSystem.API.Data;
using AttendanceSystem.API.Models;
using AttendanceSystem.API.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.Tests.Unit;

public class ApiKeyServiceTests
{
    private readonly AppDbContext _db;
    private readonly ApiKeyService _service;

    public ApiKeyServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"ApiKeyTests_{Guid.NewGuid()}")
            .Options;

        _db = new AppDbContext(options);
        _service = new ApiKeyService(_db);
    }

    [Fact]
    public void GenerateKey_ReturnsValidFormat()
    {
        var orgId = Guid.NewGuid();
        var (rawKey, entity) = _service.GenerateKey(orgId, "Test Key");

        rawKey.Should().StartWith("ak_");
        rawKey.Length.Should().BeGreaterThan(10);
        entity.KeyPrefix.Should().Be(rawKey[..11]);
        entity.OrganizationId.Should().Be(orgId);
        entity.Name.Should().Be("Test Key");
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void HashKey_IsDeterministic()
    {
        var key = "ak_testkey12345";
        var hash1 = ApiKeyService.HashKey(key);
        var hash2 = ApiKeyService.HashKey(key);

        hash1.Should().Be(hash2);
        hash1.Should().HaveLength(64); // SHA-256 hex
    }

    [Fact]
    public void HashKey_DifferentKeys_ProduceDifferentHashes()
    {
        var hash1 = ApiKeyService.HashKey("ak_key_one");
        var hash2 = ApiKeyService.HashKey("ak_key_two");

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public async Task ValidateKey_ValidKey_ReturnsOrgInfo()
    {
        var orgId = Guid.NewGuid();
        _db.Organizations.Add(new Organization
        {
            Id = orgId,
            Name = "Test Org",
            Slug = "test-org",
            CreatedAt = DateTime.UtcNow
        });

        var (rawKey, entity) = _service.GenerateKey(orgId, "Validate Test");
        _db.ApiKeys.Add(entity);
        await _db.SaveChangesAsync();

        var result = await _service.ValidateKeyAsync(rawKey);

        result.Should().NotBeNull();
        result!.Value.organizationId.Should().Be(orgId);
        result.Value.orgName.Should().Be("Test Org");
    }

    [Fact]
    public async Task ValidateKey_InvalidKey_ReturnsNull()
    {
        var result = await _service.ValidateKeyAsync("ak_invalid_key_that_doesnt_exist");

        result.Should().BeNull();
    }
}
