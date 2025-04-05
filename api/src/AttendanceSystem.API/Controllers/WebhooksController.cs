using System.Security.Cryptography;
using AttendanceSystem.API.Data;
using AttendanceSystem.API.DTOs;
using AttendanceSystem.API.Models;
using AttendanceSystem.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhooksController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IOrganizationContext _orgContext;

    public WebhooksController(AppDbContext db, IOrganizationContext orgContext)
    {
        _db = db;
        _orgContext = orgContext;
    }

    [HttpGet]
    public async Task<ActionResult<List<WebhookDto>>> GetAll()
    {
        var webhooks = await _db.Set<WebhookEndpoint>()
            .Where(w => w.OrganizationId == _orgContext.OrganizationId)
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => new WebhookDto
            {
                Id = w.Id,
                Url = w.Url,
                Events = w.Events,
                IsActive = w.IsActive,
                CreatedAt = w.CreatedAt,
                LastTriggeredAt = w.LastTriggeredAt
            })
            .ToListAsync();

        return Ok(webhooks);
    }

    [HttpPost]
    public async Task<ActionResult<WebhookDto>> Create([FromBody] CreateWebhookDto dto)
    {
        var secret = Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(32));

        var webhook = new WebhookEndpoint
        {
            Id = Guid.NewGuid(),
            OrganizationId = _orgContext.OrganizationId,
            Url = dto.Url,
            Secret = secret,
            Events = dto.Events,
            CreatedAt = DateTime.UtcNow
        };

        _db.Set<WebhookEndpoint>().Add(webhook);
        await _db.SaveChangesAsync();

        return Created("", new
        {
            webhook.Id,
            webhook.Url,
            webhook.Events,
            webhook.IsActive,
            webhook.CreatedAt,
            Secret = secret // Only shown once
        });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var webhook = await _db.Set<WebhookEndpoint>()
            .FirstOrDefaultAsync(w => w.Id == id && w.OrganizationId == _orgContext.OrganizationId);

        if (webhook is null) return NotFound();

        _db.Set<WebhookEndpoint>().Remove(webhook);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
