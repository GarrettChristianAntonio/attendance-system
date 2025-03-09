using AttendanceSystem.API.Data;
using AttendanceSystem.API.DTOs;
using AttendanceSystem.API.Models;
using AttendanceSystem.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SchedulesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IOrganizationContext _orgContext;

    public SchedulesController(AppDbContext db, IOrganizationContext orgContext)
    {
        _db = db;
        _orgContext = orgContext;
    }

    [HttpPost]
    public async Task<ActionResult<ScheduleDto>> Create([FromBody] CreateScheduleDto dto)
    {
        var employee = await _db.Employees
            .FirstOrDefaultAsync(e => e.Id == dto.EmployeeId && e.OrganizationId == _orgContext.OrganizationId && e.IsActive);
        if (employee is null) return BadRequest(new { error = "Employee not found" });

        var shift = await _db.Shifts
            .FirstOrDefaultAsync(s => s.Id == dto.ShiftId && s.OrganizationId == _orgContext.OrganizationId && s.IsActive);
        if (shift is null) return BadRequest(new { error = "Shift not found" });

        if (!DateOnly.TryParse(dto.EffectiveFrom, out var effectiveFrom))
            return BadRequest(new { error = "Invalid EffectiveFrom date" });

        DateOnly? effectiveTo = null;
        if (!string.IsNullOrEmpty(dto.EffectiveTo))
        {
            if (!DateOnly.TryParse(dto.EffectiveTo, out var parsedTo))
                return BadRequest(new { error = "Invalid EffectiveTo date" });
            effectiveTo = parsedTo;
        }

        DateOnly? specificDate = null;
        if (!string.IsNullOrEmpty(dto.SpecificDate))
        {
            if (!DateOnly.TryParse(dto.SpecificDate, out var parsedDate))
                return BadRequest(new { error = "Invalid SpecificDate" });
            specificDate = parsedDate;
        }

        var schedule = new EmployeeSchedule
        {
            Id = Guid.NewGuid(),
            EmployeeId = dto.EmployeeId,
            ShiftId = dto.ShiftId,
            DayOfWeek = dto.DayOfWeek,
            SpecificDate = specificDate,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo,
            CreatedAt = DateTime.UtcNow
        };

        _db.EmployeeSchedules.Add(schedule);
        await _db.SaveChangesAsync();

        return Created("", new ScheduleDto
        {
            Id = schedule.Id,
            EmployeeId = employee.Id,
            EmployeeName = employee.Name,
            ShiftId = shift.Id,
            ShiftName = shift.Name,
            ShiftColor = shift.Color,
            DayOfWeek = schedule.DayOfWeek,
            SpecificDate = schedule.SpecificDate?.ToString("yyyy-MM-dd"),
            EffectiveFrom = schedule.EffectiveFrom.ToString("yyyy-MM-dd"),
            EffectiveTo = schedule.EffectiveTo?.ToString("yyyy-MM-dd")
        });
    }

    [HttpGet]
    public async Task<ActionResult<WeeklyScheduleDto>> GetWeekly(
        [FromQuery] string? week,
        [FromQuery] Guid? employeeId)
    {
        DateOnly weekStart;
        if (!string.IsNullOrEmpty(week) && DateOnly.TryParse(week, out var parsed))
        {
            weekStart = parsed;
            var offset = (int)weekStart.DayOfWeek;
            weekStart = weekStart.AddDays(-offset); // Align to Sunday
        }
        else
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            weekStart = today.AddDays(-(int)today.DayOfWeek);
        }

        var weekEnd = weekStart.AddDays(7);

        var query = _db.EmployeeSchedules
            .Include(s => s.Employee)
            .Include(s => s.Shift)
            .Where(s => s.Employee.OrganizationId == _orgContext.OrganizationId
                && s.Employee.IsActive
                && s.Shift.IsActive
                && s.EffectiveFrom <= weekEnd
                && (s.EffectiveTo == null || s.EffectiveTo >= weekStart));

        if (employeeId.HasValue)
            query = query.Where(s => s.EmployeeId == employeeId.Value);

        var schedules = await query.ToListAsync();

        var grouped = schedules
            .GroupBy(s => s.EmployeeId)
            .Select(g =>
            {
                var emp = g.First().Employee;
                var days = new Dictionary<int, List<ShiftSlot>>();

                for (int d = 0; d < 7; d++)
                {
                    var currentDate = weekStart.AddDays(d);
                    var daySchedules = g.Where(s =>
                        s.SpecificDate == currentDate ||
                        (s.SpecificDate == null && s.DayOfWeek == d))
                        .Select(s => new ShiftSlot
                        {
                            ScheduleId = s.Id,
                            ShiftId = s.ShiftId,
                            ShiftName = s.Shift.Name,
                            ShiftColor = s.Shift.Color,
                            StartTime = s.Shift.StartTime.ToString("HH:mm"),
                            EndTime = s.Shift.EndTime.ToString("HH:mm")
                        })
                        .ToList();

                    if (daySchedules.Count > 0)
                        days[d] = daySchedules;
                }

                return new ScheduleEntry
                {
                    EmployeeId = emp.Id,
                    EmployeeName = emp.Name,
                    PhotoUrl = emp.PhotoPath,
                    Days = days
                };
            })
            .ToList();

        return Ok(new WeeklyScheduleDto
        {
            WeekStart = weekStart.ToString("yyyy-MM-dd"),
            Entries = grouped
        });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var schedule = await _db.EmployeeSchedules
            .Include(s => s.Employee)
            .FirstOrDefaultAsync(s => s.Id == id && s.Employee.OrganizationId == _orgContext.OrganizationId);

        if (schedule is null) return NotFound();

        _db.EmployeeSchedules.Remove(schedule);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
