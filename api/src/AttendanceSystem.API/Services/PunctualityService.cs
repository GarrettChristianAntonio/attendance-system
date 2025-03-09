using AttendanceSystem.API.Data;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.API.Services;

public class PunctualityService
{
    private readonly AppDbContext _db;

    public PunctualityService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(string status, Guid? shiftId)> EvaluateCheckIn(Guid employeeId, DateTime checkInTime)
    {
        var today = DateOnly.FromDateTime(checkInTime);
        var dayOfWeek = (int)checkInTime.DayOfWeek;

        var schedule = await _db.EmployeeSchedules
            .Include(s => s.Shift)
            .Where(s => s.EmployeeId == employeeId
                && s.Shift.IsActive
                && s.EffectiveFrom <= today
                && (s.EffectiveTo == null || s.EffectiveTo >= today)
                && (s.SpecificDate == today || (s.SpecificDate == null && s.DayOfWeek == dayOfWeek)))
            .OrderByDescending(s => s.SpecificDate.HasValue)
            .FirstOrDefaultAsync();

        if (schedule is null)
            return ("OnTime", null);

        var shift = schedule.Shift;
        var checkInTimeOnly = TimeOnly.FromDateTime(checkInTime);
        var lateThreshold = shift.StartTime.AddMinutes(shift.GracePeriodMinutes);

        var status = checkInTimeOnly <= lateThreshold ? "OnTime" : "Late";
        return (status, shift.Id);
    }
}
