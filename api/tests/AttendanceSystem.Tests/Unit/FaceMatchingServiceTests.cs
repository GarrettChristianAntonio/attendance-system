using System.Text.Json;
using AttendanceSystem.API.Data;
using AttendanceSystem.API.Models;
using AttendanceSystem.API.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.Tests.Unit;

public class FaceMatchingServiceTests
{
    private readonly AppDbContext _db;
    private readonly FaceMatchingService _service;
    private readonly Guid _orgId = Guid.NewGuid();

    public FaceMatchingServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"FaceMatchTests_{Guid.NewGuid()}")
            .Options;

        _db = new AppDbContext(options);
        _service = new FaceMatchingService(_db);
    }

    private float[] CreateDescriptor(float baseValue = 0.1f)
    {
        var descriptor = new float[128];
        for (int i = 0; i < 128; i++)
            descriptor[i] = baseValue + (i * 0.001f);
        return descriptor;
    }

    private async Task SeedEmployee(string name, float[] descriptor)
    {
        _db.Employees.Add(new Employee
        {
            Id = Guid.NewGuid(),
            OrganizationId = _orgId,
            Name = name,
            PhotoPath = "/uploads/photos/test.jpg",
            FaceDescriptor = JsonSerializer.Serialize(descriptor),
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }

    [Fact]
    public async Task FindMatch_ExactDescriptor_ReturnsMatch()
    {
        var descriptor = CreateDescriptor(0.1f);
        await SeedEmployee("Alice", descriptor);

        var result = await _service.FindMatchAsync(descriptor, _orgId);

        result.IsMatch.Should().BeTrue();
        result.Employee.Should().NotBeNull();
        result.Employee!.Name.Should().Be("Alice");
        result.Distance.Should().Be(0);
    }

    [Fact]
    public async Task FindMatch_CloseDescriptor_ReturnsMatch()
    {
        var stored = CreateDescriptor(0.1f);
        await SeedEmployee("Bob", stored);

        var query = CreateDescriptor(0.1f);
        query[0] += 0.01f;
        query[1] += 0.01f;

        var result = await _service.FindMatchAsync(query, _orgId);

        result.IsMatch.Should().BeTrue();
        result.Employee!.Name.Should().Be("Bob");
        result.Distance.Should().BeLessThan(0.55);
    }

    [Fact]
    public async Task FindMatch_DifferentDescriptor_ReturnsNoMatch()
    {
        var stored = CreateDescriptor(0.1f);
        await SeedEmployee("Charlie", stored);

        var query = CreateDescriptor(0.9f);

        var result = await _service.FindMatchAsync(query, _orgId);

        result.IsMatch.Should().BeFalse();
        result.Employee.Should().BeNull();
    }

    [Fact]
    public async Task FindMatch_ThresholdBoundary_RespectsThreshold()
    {
        var stored = CreateDescriptor(0.0f);
        await SeedEmployee("Dana", stored);

        // Create a descriptor that produces distance just above threshold
        var query = new float[128];
        // Euclidean distance of 128 values each differing by ~0.049 ≈ 0.554 > 0.55
        for (int i = 0; i < 128; i++)
            query[i] = stored[i] + 0.049f;

        var result = await _service.FindMatchAsync(query, _orgId);

        result.IsMatch.Should().BeFalse();
    }

    [Fact]
    public async Task FindMatch_EmptyDatabase_ReturnsNoMatch()
    {
        var descriptor = CreateDescriptor();

        var result = await _service.FindMatchAsync(descriptor, _orgId);

        result.IsMatch.Should().BeFalse();
        result.Employee.Should().BeNull();
    }

    [Fact]
    public async Task FindMatch_DifferentOrg_DoesNotCrossMatch()
    {
        var descriptor = CreateDescriptor(0.1f);
        await SeedEmployee("Eve", descriptor);

        var otherOrgId = Guid.NewGuid();
        var result = await _service.FindMatchAsync(descriptor, otherOrgId);

        result.IsMatch.Should().BeFalse();
    }
}
