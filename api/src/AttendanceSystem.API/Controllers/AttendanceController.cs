using AttendanceSystem.API.Data;
using AttendanceSystem.API.DTOs;
using AttendanceSystem.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AttendanceController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IOrganizationContext _orgContext;

    public AttendanceController(AppDbContext db, IOrganizationContext orgContext)
    {
        _db = db;
        _orgContext = orgContext;
    }

    [HttpGet]
    public async Task<ActionResult<List<AttendanceDto>>> GetAll(
        [FromQuery] DateTime? date,
        [FromQuery] Guid? employeeId)
    {
        var query = _db.AttendanceRecords
            .Include(a => a.Employee)
            .Include(a => a.Shift)
            .Where(a => a.Employee.OrganizationId == _orgContext.OrganizationId)
            .AsQueryable();

        if (date.HasValue)
        {
            var start = date.Value.Date;
            var end = start.AddDays(1);
            query = query.Where(a => a.CheckInAt >= start && a.CheckInAt < end);
        }

        if (employeeId.HasValue)
            query = query.Where(a => a.EmployeeId == employeeId.Value);

        var records = await query
            .OrderByDescending(a => a.CheckInAt)
            .Select(a => new AttendanceDto
            {
                Id = a.Id,
                EmployeeId = a.EmployeeId,
                EmployeeName = a.Employee.Name,
                PhotoUrl = a.Employee.PhotoPath,
                CheckInAt = a.CheckInAt,
                CheckOutAt = a.CheckOutAt,
                Confidence = a.Confidence,
                Status = a.Status,
                ShiftName = a.Shift != null ? a.Shift.Name : null,
                ShiftColor = a.Shift != null ? a.Shift.Color : null
            })
            .ToListAsync();

        return Ok(records);
    }

    [HttpGet("today")]
    public async Task<ActionResult<List<AttendanceDto>>> GetToday()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var records = await _db.AttendanceRecords
            .Include(a => a.Employee)
            .Include(a => a.Shift)
            .Where(a => a.CheckInAt >= today && a.CheckInAt < tomorrow
                     && a.Employee.OrganizationId == _orgContext.OrganizationId)
            .OrderByDescending(a => a.CheckInAt)
            .Select(a => new AttendanceDto
            {
                Id = a.Id,
                EmployeeId = a.EmployeeId,
                EmployeeName = a.Employee.Name,
                PhotoUrl = a.Employee.PhotoPath,
                CheckInAt = a.CheckInAt,
                CheckOutAt = a.CheckOutAt,
                Confidence = a.Confidence,
                Status = a.Status,
                ShiftName = a.Shift != null ? a.Shift.Name : null,
                ShiftColor = a.Shift != null ? a.Shift.Color : null
            })
            .ToListAsync();

        return Ok(records);
    }

    [HttpPost("{id:guid}/checkout")]
    public async Task<IActionResult> CheckOut(Guid id, [FromServices] CheckOutService checkOutService)
    {
        var success = await checkOutService.ManualCheckOut(id, _orgContext.OrganizationId);
        if (!success) return NotFound();
        return Ok();
    }

    [HttpGet("summary")]
    public async Task<ActionResult> GetSummary([FromQuery] string? from, [FromQuery] string? to)
    {
        var fromDate = !string.IsNullOrEmpty(from) && DateOnly.TryParse(from, out var f)
            ? f : DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var toDate = !string.IsNullOrEmpty(to) && DateOnly.TryParse(to, out var t)
            ? t : DateOnly.FromDateTime(DateTime.UtcNow);

        var fromDateTime = fromDate.ToDateTime(TimeOnly.MinValue);
        var toDateTime = toDate.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var employees = await _db.Employees
            .Where(e => e.OrganizationId == _orgContext.OrganizationId && e.IsActive)
            .ToListAsync();

        var attendanceRecords = await _db.AttendanceRecords
            .Include(a => a.Employee)
            .Where(a => a.Employee.OrganizationId == _orgContext.OrganizationId
                && a.CheckInAt >= fromDateTime && a.CheckInAt < toDateTime)
            .ToListAsync();

        var absences = await _db.AbsenceRecords
            .Include(a => a.Employee)
            .Where(a => a.Employee.OrganizationId == _orgContext.OrganizationId
                && a.Date >= fromDate && a.Date <= toDate)
            .ToListAsync();

        var summary = employees.Select(emp =>
        {
            var empRecords = attendanceRecords.Where(a => a.EmployeeId == emp.Id).ToList();
            var empAbsences = absences.Where(a => a.EmployeeId == emp.Id).ToList();
            var totalHours = empRecords
                .Where(r => r.CheckOutAt.HasValue)
                .Sum(r => (r.CheckOutAt!.Value - r.CheckInAt).TotalHours);

            return new
            {
                EmployeeId = emp.Id,
                EmployeeName = emp.Name,
                PhotoUrl = emp.PhotoPath,
                TotalDays = empRecords.Select(r => r.CheckInAt.Date).Distinct().Count(),
                OnTimeCount = empRecords.Count(r => r.Status == "OnTime"),
                LateCount = empRecords.Count(r => r.Status == "Late"),
                EarlyDepartureCount = empRecords.Count(r => r.Status == "EarlyDeparture"),
                AbsentCount = empAbsences.Count(a => a.Type == "Absent"),
                TotalHours = Math.Round(totalHours, 1),
                AvgArrivalTime = empRecords.Count > 0
                    ? TimeOnly.FromTimeSpan(TimeSpan.FromTicks(
                        (long)empRecords.Average(r => r.CheckInAt.TimeOfDay.Ticks))).ToString("HH:mm")
                    : null
            };
        }).ToList();

        return Ok(summary);
    }
}
