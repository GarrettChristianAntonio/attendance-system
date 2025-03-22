using AttendanceSystem.API.Data;
using AttendanceSystem.API.DTOs;
using AttendanceSystem.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IOrganizationContext _orgContext;

    public AnalyticsController(AppDbContext db, IOrganizationContext orgContext)
    {
        _db = db;
        _orgContext = orgContext;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardDto>> GetDashboard()
    {
        var orgId = _orgContext.OrganizationId;
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var weekAgo = today.AddDays(-6);

        var totalEmployees = await _db.Employees
            .CountAsync(e => e.OrganizationId == orgId && e.IsActive);

        var todayRecords = await _db.AttendanceRecords
            .Include(a => a.Employee)
            .Where(a => a.Employee.OrganizationId == orgId
                && a.CheckInAt >= today && a.CheckInAt < tomorrow)
            .ToListAsync();

        var todayCheckIns = todayRecords.Count;
        var onTimeToday = todayRecords.Count(r => r.Status == "OnTime");
        var lateToday = todayRecords.Count(r => r.Status == "Late");
        var onTimeRate = todayCheckIns > 0 ? Math.Round((double)onTimeToday / todayCheckIns * 100, 1) : 100;

        // Weekly stats
        var weekRecords = await _db.AttendanceRecords
            .Include(a => a.Employee)
            .Where(a => a.Employee.OrganizationId == orgId
                && a.CheckInAt >= weekAgo && a.CheckInAt < tomorrow)
            .ToListAsync();

        var weekAbsences = await _db.AbsenceRecords
            .Include(a => a.Employee)
            .Where(a => a.Employee.OrganizationId == orgId
                && a.Date >= DateOnly.FromDateTime(weekAgo)
                && a.Date <= DateOnly.FromDateTime(today))
            .ToListAsync();

        var weeklyStats = new List<DailyStatsDto>();
        for (int i = 6; i >= 0; i--)
        {
            var d = today.AddDays(-i);
            var dayRecords = weekRecords.Where(r => r.CheckInAt.Date == d).ToList();
            var dayAbsences = weekAbsences.Where(a => a.Date == DateOnly.FromDateTime(d)).ToList();
            weeklyStats.Add(new DailyStatsDto
            {
                Date = d.ToString("yyyy-MM-dd"),
                DayLabel = d.ToString("ddd"),
                CheckIns = dayRecords.Count,
                OnTime = dayRecords.Count(r => r.Status == "OnTime"),
                Late = dayRecords.Count(r => r.Status == "Late"),
                Absent = dayAbsences.Count
            });
        }

        // Top employees by on-time rate (last 30 days)
        var monthAgo = today.AddDays(-30);
        var monthRecords = await _db.AttendanceRecords
            .Include(a => a.Employee)
            .Where(a => a.Employee.OrganizationId == orgId
                && a.Employee.IsActive
                && a.CheckInAt >= monthAgo)
            .ToListAsync();

        var topEmployees = monthRecords
            .GroupBy(r => r.EmployeeId)
            .Select(g =>
            {
                var emp = g.First().Employee;
                var total = g.Count();
                var onTime = g.Count(r => r.Status == "OnTime");
                return new TopEmployeeDto
                {
                    EmployeeId = emp.Id,
                    EmployeeName = emp.Name,
                    PhotoUrl = emp.PhotoPath,
                    OnTimeCount = onTime,
                    TotalDays = g.Select(r => r.CheckInAt.Date).Distinct().Count(),
                    OnTimeRate = total > 0 ? Math.Round((double)onTime / total * 100, 1) : 0
                };
            })
            .OrderByDescending(e => e.OnTimeRate)
            .ThenByDescending(e => e.TotalDays)
            .Take(5)
            .ToList();

        return Ok(new DashboardDto
        {
            TotalEmployees = totalEmployees,
            TodayCheckIns = todayCheckIns,
            OnTimeToday = onTimeToday,
            LateToday = lateToday,
            OnTimeRate = onTimeRate,
            WeeklyStats = weeklyStats,
            TopEmployees = topEmployees
        });
    }

    [HttpGet("employee/{id:guid}")]
    public async Task<ActionResult<EmployeeAnalyticsDto>> GetEmployeeAnalytics(Guid id, [FromQuery] string? from, [FromQuery] string? to)
    {
        var orgId = _orgContext.OrganizationId;
        var employee = await _db.Employees
            .FirstOrDefaultAsync(e => e.Id == id && e.OrganizationId == orgId);

        if (employee is null) return NotFound();

        var fromDate = !string.IsNullOrEmpty(from) && DateOnly.TryParse(from, out var f)
            ? f : DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var toDate = !string.IsNullOrEmpty(to) && DateOnly.TryParse(to, out var t)
            ? t : DateOnly.FromDateTime(DateTime.UtcNow);

        var fromDateTime = fromDate.ToDateTime(TimeOnly.MinValue);
        var toDateTime = toDate.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var records = await _db.AttendanceRecords
            .Where(a => a.EmployeeId == id && a.CheckInAt >= fromDateTime && a.CheckInAt < toDateTime)
            .OrderBy(a => a.CheckInAt)
            .ToListAsync();

        var absences = await _db.AbsenceRecords
            .Where(a => a.EmployeeId == id && a.Date >= fromDate && a.Date <= toDate)
            .ToListAsync();

        var totalHours = records
            .Where(r => r.CheckOutAt.HasValue)
            .Sum(r => (r.CheckOutAt!.Value - r.CheckInAt).TotalHours);

        var dailyHistory = records
            .GroupBy(r => r.CheckInAt.Date)
            .Select(g => new DailyStatsDto
            {
                Date = g.Key.ToString("yyyy-MM-dd"),
                DayLabel = g.Key.ToString("ddd"),
                CheckIns = g.Count(),
                OnTime = g.Count(r => r.Status == "OnTime"),
                Late = g.Count(r => r.Status == "Late"),
                Absent = 0
            })
            .ToList();

        return Ok(new EmployeeAnalyticsDto
        {
            EmployeeId = employee.Id,
            EmployeeName = employee.Name,
            PhotoUrl = employee.PhotoPath,
            TotalDays = records.Select(r => r.CheckInAt.Date).Distinct().Count(),
            OnTimeCount = records.Count(r => r.Status == "OnTime"),
            LateCount = records.Count(r => r.Status == "Late"),
            AbsentCount = absences.Count(a => a.Type == "Absent"),
            EarlyDepartureCount = records.Count(r => r.Status == "EarlyDeparture"),
            TotalHours = Math.Round(totalHours, 1),
            AvgArrivalTime = records.Count > 0
                ? TimeOnly.FromTimeSpan(TimeSpan.FromTicks(
                    (long)records.Average(r => r.CheckInAt.TimeOfDay.Ticks))).ToString("HH:mm")
                : null,
            DailyHistory = dailyHistory
        });
    }
}
