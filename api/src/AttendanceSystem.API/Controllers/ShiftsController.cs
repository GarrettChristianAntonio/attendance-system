using AttendanceSystem.API.Data;
using AttendanceSystem.API.DTOs;
using AttendanceSystem.API.Models;
using AttendanceSystem.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShiftsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IOrganizationContext _orgContext;

    public ShiftsController(AppDbContext db, IOrganizationContext orgContext)
    {
        _db = db;
        _orgContext = orgContext;
    }

    [HttpGet]
    public async Task<ActionResult<List<ShiftDto>>> GetAll()
    {
        var shifts = await _db.Shifts
            .Where(s => s.OrganizationId == _orgContext.OrganizationId && s.IsActive)
            .OrderBy(s => s.StartTime)
            .Select(s => new ShiftDto
            {
                Id = s.Id,
                Name = s.Name,
                StartTime = s.StartTime.ToString("HH:mm"),
                EndTime = s.EndTime.ToString("HH:mm"),
                GracePeriodMinutes = s.GracePeriodMinutes,
                Color = s.Color,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();

        return Ok(shifts);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ShiftDto>> GetById(Guid id)
    {
        var shift = await _db.Shifts
            .FirstOrDefaultAsync(s => s.Id == id && s.OrganizationId == _orgContext.OrganizationId);

        if (shift is null) return NotFound();

        return Ok(new ShiftDto
        {
            Id = shift.Id,
            Name = shift.Name,
            StartTime = shift.StartTime.ToString("HH:mm"),
            EndTime = shift.EndTime.ToString("HH:mm"),
            GracePeriodMinutes = shift.GracePeriodMinutes,
            Color = shift.Color,
            IsActive = shift.IsActive,
            CreatedAt = shift.CreatedAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<ShiftDto>> Create([FromBody] CreateShiftDto dto)
    {
        if (!TimeOnly.TryParse(dto.StartTime, out var startTime) ||
            !TimeOnly.TryParse(dto.EndTime, out var endTime))
            return BadRequest(new { error = "Invalid time format. Use HH:mm" });

        var shift = new Shift
        {
            Id = Guid.NewGuid(),
            OrganizationId = _orgContext.OrganizationId,
            Name = dto.Name,
            StartTime = startTime,
            EndTime = endTime,
            GracePeriodMinutes = dto.GracePeriodMinutes,
            Color = dto.Color,
            CreatedAt = DateTime.UtcNow
        };

        _db.Shifts.Add(shift);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = shift.Id }, new ShiftDto
        {
            Id = shift.Id,
            Name = shift.Name,
            StartTime = shift.StartTime.ToString("HH:mm"),
            EndTime = shift.EndTime.ToString("HH:mm"),
            GracePeriodMinutes = shift.GracePeriodMinutes,
            Color = shift.Color,
            IsActive = shift.IsActive,
            CreatedAt = shift.CreatedAt
        });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ShiftDto>> Update(Guid id, [FromBody] CreateShiftDto dto)
    {
        var shift = await _db.Shifts
            .FirstOrDefaultAsync(s => s.Id == id && s.OrganizationId == _orgContext.OrganizationId);

        if (shift is null) return NotFound();

        if (!TimeOnly.TryParse(dto.StartTime, out var startTime) ||
            !TimeOnly.TryParse(dto.EndTime, out var endTime))
            return BadRequest(new { error = "Invalid time format. Use HH:mm" });

        shift.Name = dto.Name;
        shift.StartTime = startTime;
        shift.EndTime = endTime;
        shift.GracePeriodMinutes = dto.GracePeriodMinutes;
        shift.Color = dto.Color;

        await _db.SaveChangesAsync();

        return Ok(new ShiftDto
        {
            Id = shift.Id,
            Name = shift.Name,
            StartTime = shift.StartTime.ToString("HH:mm"),
            EndTime = shift.EndTime.ToString("HH:mm"),
            GracePeriodMinutes = shift.GracePeriodMinutes,
            Color = shift.Color,
            IsActive = shift.IsActive,
            CreatedAt = shift.CreatedAt
        });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var shift = await _db.Shifts
            .FirstOrDefaultAsync(s => s.Id == id && s.OrganizationId == _orgContext.OrganizationId);

        if (shift is null) return NotFound();

        shift.IsActive = false;
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
