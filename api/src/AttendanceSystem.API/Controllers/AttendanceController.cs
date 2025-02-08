using AttendanceSystem.API.Data;
using AttendanceSystem.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AttendanceController : ControllerBase
{
    private readonly AppDbContext _db;

    public AttendanceController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<AttendanceDto>>> GetAll(
        [FromQuery] DateTime? date,
        [FromQuery] Guid? employeeId)
    {
        var query = _db.AttendanceRecords
            .Include(a => a.Employee)
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
                Confidence = a.Confidence
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
            .Where(a => a.CheckInAt >= today && a.CheckInAt < tomorrow)
            .OrderByDescending(a => a.CheckInAt)
            .Select(a => new AttendanceDto
            {
                Id = a.Id,
                EmployeeId = a.EmployeeId,
                EmployeeName = a.Employee.Name,
                PhotoUrl = a.Employee.PhotoPath,
                CheckInAt = a.CheckInAt,
                Confidence = a.Confidence
            })
            .ToListAsync();

        return Ok(records);
    }
}
