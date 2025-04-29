using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AttendanceSystem.API.Data;
using AttendanceSystem.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AttendanceSystem.API.Services;

public class EmbedTokenService
{
    private readonly AppDbContext _db;
    private readonly string _jwtSecret;

    public EmbedTokenService(AppDbContext db, IConfiguration configuration)
    {
        _db = db;
        _jwtSecret = configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret is not configured");
    }

    public async Task<(string jwt, EmbedToken entity)> GenerateToken(
        Guid organizationId, string name, string[] scopes, int expiresInDays)
    {
        var tokenId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(expiresInDays);
        var scopeString = string.Join(",", scopes);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("token_id", tokenId.ToString()),
            new Claim("org_id", organizationId.ToString()),
            new Claim("scope", scopeString)
        };

        var token = new JwtSecurityToken(
            issuer: "attendance-system",
            audience: "embed",
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        var secretHash = HashToken(jwt);

        var entity = new EmbedToken
        {
            Id = tokenId,
            OrganizationId = organizationId,
            Name = name,
            SecretHash = secretHash,
            Scopes = scopeString,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        };

        _db.EmbedTokens.Add(entity);
        await _db.SaveChangesAsync();

        return (jwt, entity);
    }

    public (Guid organizationId, string[] scopes)? ValidateToken(string jwt)
    {
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
            var handler = new JwtSecurityTokenHandler();
            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "attendance-system",
                ValidateAudience = true,
                ValidAudience = "embed",
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            };

            var principal = handler.ValidateToken(jwt, parameters, out _);
            var orgId = principal.FindFirst("org_id")?.Value;
            var scope = principal.FindFirst("scope")?.Value;

            if (orgId == null || !Guid.TryParse(orgId, out var parsedOrgId))
                return null;

            var scopes = string.IsNullOrEmpty(scope)
                ? Array.Empty<string>()
                : scope.Split(',', StringSplitOptions.RemoveEmptyEntries);

            return (parsedOrgId, scopes);
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<EmbedToken>> GetTokensForOrg(Guid organizationId)
    {
        return await _db.EmbedTokens
            .Where(t => t.OrganizationId == organizationId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> RevokeToken(Guid tokenId, Guid organizationId)
    {
        var token = await _db.EmbedTokens
            .FirstOrDefaultAsync(t => t.Id == tokenId && t.OrganizationId == organizationId);

        if (token == null) return false;

        _db.EmbedTokens.Remove(token);
        await _db.SaveChangesAsync();
        return true;
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexStringLower(bytes);
    }
}
