using AttendanceSystem.API.Data;
using AttendanceSystem.API.Models;
using AttendanceSystem.API.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.Tests.Unit;

public class PunctualityServiceTests
{
    private readonly AppDbContext _db;
    private readonly PunctualityService _service;
    private readonly Guid _orgId = Guid.NewGuid();

    public PunctualityServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"PunctualityTests_{Guid.NewGuid()}")
            .Options;

        _db = new AppDbContext(options);
        _service = new PunctualityService(_db);
    }

    private async Task<(Guid employeeId, Guid shiftId)> SeedEmployeeWithShift(
        TimeOnly shiftStart, TimeOnly shiftEnd, int gracePeriod = 15, int dayOfWeek = 1)
    {
        var employeeId = Guid.NewGuid();
        var shiftId = Guid.NewGuid();

        _db.Employees.Add(new Employee
        {
            Id = employeeId,
            OrganizationId = _orgId,
            Name = "Test Employee",
            PhotoPath = "/test.jpg",
            FaceDescriptor = "[]",
            CreatedAt = DateTime.UtcNow
        });

        _db.Shifts.Add(new Shift
        {
            Id = shiftId,
            OrganizationId = _orgId,
            Name = "Morning Shift",
            StartTime = shiftStart,
            EndTime = shiftEnd,
            GracePeriodMinutes = gracePeriod,
            Color = "#3B82F6",
            CreatedAt = DateTime.UtcNow
        });

        _db.EmployeeSchedules.Add(new EmployeeSchedule
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            ShiftId = shiftId,
            DayOfWeek = dayOfWeek,
            EffectiveFrom = DateOnly.MinValue,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return (employeeId, shiftId);
    }

    [Fact]
    public async Task EvaluateCheckIn_OnTime_ReturnsOnTime()
    {
        // Monday shift 09:00-17:00, grace 15 min
        var (employeeId, shiftId) = await SeedEmployeeWithShift(
            new TimeOnly(9, 0), new TimeOnly(17, 0), dayOfWeek: 1);

        // Check in at 09:00 on a Monday
        var monday = GetNextDayOfWeek(DayOfWeek.Monday);
        var checkIn = monday.AddHours(9);

        var (status, resultShiftId) = await _service.EvaluateCheckIn(employeeId, checkIn);

        status.Should().Be("OnTime");
        resultShiftId.Should().Be(shiftId);
    }

    [Fact]
    public async Task EvaluateCheckIn_Late_ReturnsLate()
    {
        var (employeeId, shiftId) = await SeedEmployeeWithShift(
            new TimeOnly(9, 0), new TimeOnly(17, 0), gracePeriod: 15, dayOfWeek: 1);

        var monday = GetNextDayOfWeek(DayOfWeek.Monday);
        var checkIn = monday.AddHours(9).AddMinutes(20); // 20 min late, beyond 15 min grace

        var (status, resultShiftId) = await _service.EvaluateCheckIn(employeeId, checkIn);

        status.Should().Be("Late");
        resultShiftId.Should().Be(shiftId);
    }

    [Fact]
    public async Task EvaluateCheckIn_WithinGracePeriod_ReturnsOnTime()
    {
        var (employeeId, shiftId) = await SeedEmployeeWithShift(
            new TimeOnly(9, 0), new TimeOnly(17, 0), gracePeriod: 15, dayOfWeek: 1);

        var monday = GetNextDayOfWeek(DayOfWeek.Monday);
        var checkIn = monday.AddHours(9).AddMinutes(10); // 10 min late, within 15 min grace

        var (status, _) = await _service.EvaluateCheckIn(employeeId, checkIn);

        status.Should().Be("OnTime");
    }

    [Fact]
    public async Task EvaluateCheckIn_NoShiftAssigned_ReturnsOnTime()
    {
        var employeeId = Guid.NewGuid();
        _db.Employees.Add(new Employee
        {
            Id = employeeId,
            OrganizationId = _orgId,
            Name = "No Shift Employee",
            PhotoPath = "/test.jpg",
            FaceDescriptor = "[]",
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        var (status, shiftId) = await _service.EvaluateCheckIn(employeeId, DateTime.UtcNow);

        status.Should().Be("OnTime");
        shiftId.Should().BeNull();
    }

    private static DateTime GetNextDayOfWeek(DayOfWeek day)
    {
        var date = new DateTime(2025, 4, 21, 0, 0, 0, DateTimeKind.Utc); // Known Monday
        while (date.DayOfWeek != day)
            date = date.AddDays(1);
        return date;
    }
}
