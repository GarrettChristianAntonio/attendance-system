using AttendanceSystem.API.Data;
using AttendanceSystem.API.DTOs;
using AttendanceSystem.API.Models;
using AttendanceSystem.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FaceController : ControllerBase
{
    private readonly FaceMatchingService _matchingService;
    private readonly AppDbContext _db;
    private readonly IOrganizationContext _orgContext;
    private readonly PunctualityService _punctualityService;
    private static readonly TimeSpan CooldownPeriod = TimeSpan.FromMinutes(5);

    public FaceController(
        FaceMatchingService matchingService,
        AppDbContext db,
        IOrganizationContext orgContext,
        PunctualityService punctualityService)
    {
        _matchingService = matchingService;
        _db = db;
        _orgContext = orgContext;
        _punctualityService = punctualityService;
    }

    [HttpPost("match")]
    public async Task<ActionResult<MatchResponseDto>> Match([FromBody] MatchRequestDto dto)
    {
        if (dto.Descriptor.Length != 128)
            return BadRequest("Descriptor must be a 128-float array");

        var result = await _matchingService.FindMatchAsync(dto.Descriptor, _orgContext.OrganizationId);

        if (result.IsMatch && result.Employee is not null)
        {
            var cooldownThreshold = DateTime.UtcNow - CooldownPeriod;
            var recentCheckIn = await _db.AttendanceRecords
                .AnyAsync(a => a.EmployeeId == result.Employee.Id && a.CheckInAt > cooldownThreshold);

            if (!recentCheckIn)
            {
                var now = DateTime.UtcNow;
                var (status, shiftId) = await _punctualityService.EvaluateCheckIn(result.Employee.Id, now);

                var record = new AttendanceRecord
                {
                    Id = Guid.NewGuid(),
                    EmployeeId = result.Employee.Id,
                    ShiftId = shiftId,
                    CheckInAt = now,
                    Confidence = result.Distance,
                    Status = status
                };
                _db.AttendanceRecords.Add(record);
                await _db.SaveChangesAsync();
            }
        }

        return Ok(new MatchResponseDto
        {
            IsMatch = result.IsMatch,
            EmployeeId = result.Employee?.Id,
            EmployeeName = result.Employee?.Name,
            PhotoUrl = result.Employee?.PhotoPath,
            Distance = result.Distance
        });
    }
}
