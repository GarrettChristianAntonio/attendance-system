using AttendanceSystem.API.DTOs;
using AttendanceSystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FaceController : ControllerBase
{
    private readonly FaceMatchingService _matchingService;

    public FaceController(FaceMatchingService matchingService)
    {
        _matchingService = matchingService;
    }

    [HttpPost("match")]
    public async Task<ActionResult<MatchResponseDto>> Match([FromBody] MatchRequestDto dto)
    {
        if (dto.Descriptor.Length != 128)
            return BadRequest("Descriptor must be a 128-float array");

        var result = await _matchingService.FindMatchAsync(dto.Descriptor);

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
