using AttendanceSystem.API.Data;
using AttendanceSystem.API.DTOs;
using AttendanceSystem.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApiKeysController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ApiKeyService _apiKeyService;
    private readonly IOrganizationContext _orgContext;

    public ApiKeysController(AppDbContext db, ApiKeyService apiKeyService, IOrganizationContext orgContext)
    {
        _db = db;
        _apiKeyService = apiKeyService;
        _orgContext = orgContext;
    }

    [HttpPost]
    public async Task<ActionResult<CreateApiKeyResponseDto>> Create([FromBody] CreateApiKeyDto dto)
    {
        var (rawKey, entity) = _apiKeyService.GenerateKey(_orgContext.OrganizationId, dto.Name);
        _db.ApiKeys.Add(entity);
        await _db.SaveChangesAsync();

        return Created("", new CreateApiKeyResponseDto
        {
            Key = new ApiKeyDto
            {
                Id = entity.Id,
                Name = entity.Name,
                KeyPrefix = entity.KeyPrefix,
                IsActive = entity.IsActive,
                LastUsedAt = entity.LastUsedAt,
                CreatedAt = entity.CreatedAt
            },
            RawKey = rawKey
        });
    }

    [HttpGet]
    public async Task<ActionResult<List<ApiKeyDto>>> GetAll()
    {
        var keys = await _db.ApiKeys
            .Where(k => k.OrganizationId == _orgContext.OrganizationId)
            .OrderByDescending(k => k.CreatedAt)
            .Select(k => new ApiKeyDto
            {
                Id = k.Id,
                Name = k.Name,
                KeyPrefix = k.KeyPrefix,
                IsActive = k.IsActive,
                LastUsedAt = k.LastUsedAt,
                CreatedAt = k.CreatedAt
            })
            .ToListAsync();

        return Ok(keys);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Revoke(Guid id)
    {
        var key = await _db.ApiKeys
            .FirstOrDefaultAsync(k => k.Id == id && k.OrganizationId == _orgContext.OrganizationId);

        if (key is null) return NotFound();

        key.IsActive = false;
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
