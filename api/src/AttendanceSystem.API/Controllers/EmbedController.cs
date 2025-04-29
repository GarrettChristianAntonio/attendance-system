using AttendanceSystem.API.DTOs;
using AttendanceSystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmbedController : ControllerBase
{
    private readonly EmbedTokenService _embedTokenService;
    private readonly IOrganizationContext _orgContext;

    public EmbedController(EmbedTokenService embedTokenService, IOrganizationContext orgContext)
    {
        _embedTokenService = embedTokenService;
        _orgContext = orgContext;
    }

    [HttpPost("tokens")]
    public async Task<ActionResult<EmbedTokenResponse>> CreateToken([FromBody] CreateEmbedTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "Token name is required" });

        var validScopes = new[] { "embed:checkin", "embed:status", "embed:feed" };
        var invalidScopes = request.Scopes.Except(validScopes).ToArray();
        if (invalidScopes.Length > 0)
            return BadRequest(new { error = $"Invalid scopes: {string.Join(", ", invalidScopes)}" });

        if (request.Scopes.Length == 0)
            return BadRequest(new { error = "At least one scope is required" });

        var (jwt, entity) = await _embedTokenService.GenerateToken(
            _orgContext.OrganizationId,
            request.Name,
            request.Scopes,
            request.ExpiresInDays);

        return Ok(new EmbedTokenResponse
        {
            Token = jwt,
            ExpiresAt = entity.ExpiresAt
        });
    }

    [HttpGet("tokens")]
    public async Task<ActionResult<List<EmbedTokenListItem>>> ListTokens()
    {
        var tokens = await _embedTokenService.GetTokensForOrg(_orgContext.OrganizationId);

        return Ok(tokens.Select(t => new EmbedTokenListItem
        {
            Id = t.Id,
            Name = t.Name,
            Scopes = t.Scopes,
            ExpiresAt = t.ExpiresAt,
            CreatedAt = t.CreatedAt
        }).ToList());
    }

    [HttpDelete("tokens/{id:guid}")]
    public async Task<IActionResult> RevokeToken(Guid id)
    {
        var revoked = await _embedTokenService.RevokeToken(id, _orgContext.OrganizationId);
        if (!revoked) return NotFound();
        return NoContent();
    }
}
