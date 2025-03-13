using AttendanceSystem.API.Data;
using AttendanceSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.API.Services;

public class AbsenceDetectionService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AbsenceDetectionService> _logger;

    public AbsenceDetectionService(IServiceScopeFactory scopeFactory, ILogger<AbsenceDetectionService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var nextRun = now.Date.AddDays(1).AddHours(1); // Run at 1 AM daily
            var delay = nextRun - now;
            if (delay < TimeSpan.Zero) delay = TimeSpan.FromHours(1);

            await Task.Delay(delay, stoppingToken);
            if (stoppingToken.IsCancellationRequested) break;

            try
            {
                await DetectAbsences(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting absences");
            }
        }
    }

    private async Task DetectAbsences(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var dayOfWeek = (int)yesterday.DayOfWeek;

        var scheduledEmployees = await db.EmployeeSchedules
            .Include(s => s.Employee)
            .Include(s => s.Shift)
            .Where(s => s.Employee.IsActive
                && s.Shift.IsActive
                && s.EffectiveFrom <= yesterday
                && (s.EffectiveTo == null || s.EffectiveTo >= yesterday)
                && (s.SpecificDate == yesterday || (s.SpecificDate == null && s.DayOfWeek == dayOfWeek)))
            .ToListAsync(ct);

        var yesterdayStart = yesterday.ToDateTime(TimeOnly.MinValue);
        var yesterdayEnd = yesterday.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var checkIns = await db.AttendanceRecords
            .Where(a => a.CheckInAt >= yesterdayStart && a.CheckInAt < yesterdayEnd)
            .Select(a => a.EmployeeId)
            .Distinct()
            .ToListAsync(ct);

        var checkInSet = new HashSet<Guid>(checkIns);

        foreach (var schedule in scheduledEmployees)
        {
            if (checkInSet.Contains(schedule.EmployeeId)) continue;

            var existingAbsence = await db.AbsenceRecords
                .AnyAsync(a => a.EmployeeId == schedule.EmployeeId
                    && a.ShiftId == schedule.ShiftId
                    && a.Date == yesterday, ct);

            if (existingAbsence) continue;

            db.AbsenceRecords.Add(new AbsenceRecord
            {
                Id = Guid.NewGuid(),
                EmployeeId = schedule.EmployeeId,
                ShiftId = schedule.ShiftId,
                Date = yesterday,
                Type = "Absent",
                CreatedAt = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync(ct);
        _logger.LogInformation("Absence detection completed for {Date}", yesterday);
    }
}
