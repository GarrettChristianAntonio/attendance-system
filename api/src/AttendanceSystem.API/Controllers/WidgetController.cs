using AttendanceSystem.API.Data;
using AttendanceSystem.API.DTOs;
using AttendanceSystem.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WidgetController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IOrganizationContext _orgContext;

    public WidgetController(AppDbContext db, IOrganizationContext orgContext)
    {
        _db = db;
        _orgContext = orgContext;
    }

    [HttpGet("status")]
    public async Task<ActionResult> GetStatus()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var records = await _db.AttendanceRecords
            .Include(a => a.Employee)
            .Where(a => a.Employee.OrganizationId == _orgContext.OrganizationId
                && a.CheckInAt >= today && a.CheckInAt < tomorrow)
            .OrderByDescending(a => a.CheckInAt)
            .Take(20)
            .Select(a => new
            {
                a.Employee.Name,
                a.Employee.PhotoPath,
                CheckInAt = a.CheckInAt,
                a.Status
            })
            .ToListAsync();

        var totalEmployees = await _db.Employees
            .CountAsync(e => e.OrganizationId == _orgContext.OrganizationId && e.IsActive);

        return Ok(new
        {
            TotalEmployees = totalEmployees,
            CheckedIn = records.Count,
            RecentCheckIns = records
        });
    }
}
