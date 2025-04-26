using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AttendanceSystem.API.Data;
using AttendanceSystem.API.Models;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AttendanceSystem.Tests.Integration;

public class AttendanceControllerTests : IntegrationTestBase
{
    public AttendanceControllerTests(TestWebApplicationFactory factory) : base(factory) { }

    private void SeedAttendanceRecord(DateTime checkInAt, string status = "OnTime")
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var employee = db.Employees.FirstOrDefault(e => e.OrganizationId == TestOrganizationId);
        if (employee == null)
        {
            employee = new Employee
            {
                Id = Guid.NewGuid(),
                OrganizationId = TestOrganizationId,
                Name = "Attendance Test Employee",
                PhotoPath = "/test.jpg",
                FaceDescriptor = "[]",
                CreatedAt = DateTime.UtcNow
            };
            db.Employees.Add(employee);
        }

        db.AttendanceRecords.Add(new AttendanceRecord
        {
            Id = Guid.NewGuid(),
            EmployeeId = employee.Id,
            CheckInAt = checkInAt,
            Confidence = 0.35,
            Status = status
        });
        db.SaveChanges();
    }

    [Fact]
    public async Task GetToday_ReturnsOk()
    {
        SeedAttendanceRecord(DateTime.UtcNow);

        var response = await Client.GetAsync("/api/attendance/today");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var records = await response.Content.ReadFromJsonAsync<JsonElement[]>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        records.Should().NotBeNull();
        records!.Length.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetAll_WithDateFilter_ReturnsFiltered()
    {
        var targetDate = new DateTime(2025, 3, 15, 10, 0, 0, DateTimeKind.Utc);
        SeedAttendanceRecord(targetDate);

        var response = await Client.GetAsync("/api/attendance?date=2025-03-15");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var records = await response.Content.ReadFromJsonAsync<JsonElement[]>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        records.Should().NotBeNull();
        records!.Should().AllSatisfy(r =>
        {
            var checkIn = r.GetProperty("checkInAt").GetDateTime();
            checkIn.Date.Should().Be(new DateTime(2025, 3, 15));
        });
    }

    [Fact]
    public async Task GetSummary_ReturnsOk()
    {
        SeedAttendanceRecord(DateTime.UtcNow);

        var response = await Client.GetAsync("/api/attendance/summary");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
