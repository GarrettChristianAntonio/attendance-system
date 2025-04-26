using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AttendanceSystem.API.Data;
using AttendanceSystem.API.Models;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AttendanceSystem.Tests.Integration;

public class FaceControllerTests : IntegrationTestBase
{
    public FaceControllerTests(TestWebApplicationFactory factory) : base(factory) { }

    private float[] CreateDescriptor(float baseValue = 0.1f)
    {
        var descriptor = new float[128];
        for (int i = 0; i < 128; i++)
            descriptor[i] = baseValue + (i * 0.001f);
        return descriptor;
    }

    private void SeedEmployee(float[] descriptor, string name = "Face Test Employee")
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.Employees.Add(new Employee
        {
            Id = Guid.NewGuid(),
            OrganizationId = TestOrganizationId,
            Name = name,
            PhotoPath = "/test.jpg",
            FaceDescriptor = JsonSerializer.Serialize(descriptor),
            CreatedAt = DateTime.UtcNow
        });
        db.SaveChanges();
    }

    [Fact]
    public async Task Match_KnownFace_ReturnsMatch()
    {
        var descriptor = CreateDescriptor(0.2f);
        SeedEmployee(descriptor, "Known Person");

        var response = await Client.PostAsJsonAsync("/api/face/match", new { Descriptor = descriptor });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("isMatch").GetBoolean().Should().BeTrue();
        body.GetProperty("employeeName").GetString().Should().Be("Known Person");
    }

    [Fact]
    public async Task Match_UnknownFace_ReturnsNoMatch()
    {
        var storedDescriptor = CreateDescriptor(0.1f);
        SeedEmployee(storedDescriptor, "Stored Person");

        var unknownDescriptor = CreateDescriptor(0.9f);
        var response = await Client.PostAsJsonAsync("/api/face/match", new { Descriptor = unknownDescriptor });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("isMatch").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task Match_InvalidDescriptorLength_ReturnsBadRequest()
    {
        var shortDescriptor = new float[64];
        var response = await Client.PostAsJsonAsync("/api/face/match", new { Descriptor = shortDescriptor });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
