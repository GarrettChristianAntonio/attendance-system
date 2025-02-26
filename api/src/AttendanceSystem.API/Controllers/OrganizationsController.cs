using AttendanceSystem.API.Data;
using AttendanceSystem.API.DTOs;
using AttendanceSystem.API.Models;
using AttendanceSystem.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrganizationsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ApiKeyService _apiKeyService;
    private readonly IOrganizationContext _orgContext;

    public OrganizationsController(AppDbContext db, ApiKeyService apiKeyService, IOrganizationContext orgContext)
    {
        _db = db;
        _apiKeyService = apiKeyService;
        _orgContext = orgContext;
    }

    [HttpPost("create")]
    public async Task<ActionResult<CreateOrganizationResponseDto>> Create([FromBody] CreateOrganizationDto dto)
    {
        if (await _db.Organizations.AnyAsync(o => o.Slug == dto.Slug))
            return Conflict(new { error = "An organization with this slug already exists." });

        var org = new Organization
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Slug = dto.Slug,
            CreatedAt = DateTime.UtcNow
        };

        _db.Organizations.Add(org);

        var (rawKey, keyEntity) = _apiKeyService.GenerateKey(org.Id, "Default Key");
        _db.ApiKeys.Add(keyEntity);

        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCurrent), null, new CreateOrganizationResponseDto
        {
            Organization = new OrganizationDto
            {
                Id = org.Id,
                Name = org.Name,
                Slug = org.Slug,
                IsActive = org.IsActive,
                CreatedAt = org.CreatedAt
            },
            ApiKey = rawKey
        });
    }

    [HttpGet("current")]
    public async Task<ActionResult<OrganizationDto>> GetCurrent()
    {
        var org = await _db.Organizations.FindAsync(_orgContext.OrganizationId);
        if (org is null) return NotFound();

        return Ok(new OrganizationDto
        {
            Id = org.Id,
            Name = org.Name,
            Slug = org.Slug,
            IsActive = org.IsActive,
            CreatedAt = org.CreatedAt
        });
    }
}
