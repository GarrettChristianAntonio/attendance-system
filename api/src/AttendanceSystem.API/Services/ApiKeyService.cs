using System.Security.Cryptography;
using System.Text;
using AttendanceSystem.API.Data;
using AttendanceSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.API.Services;

public class ApiKeyService
{
    private readonly AppDbContext _db;

    public ApiKeyService(AppDbContext db)
    {
        _db = db;
    }

    public (string rawKey, ApiKey entity) GenerateKey(Guid organizationId, string name)
    {
        var rawBytes = RandomNumberGenerator.GetBytes(32);
        var rawKey = "ak_" + Convert.ToBase64String(rawBytes)
            .Replace("+", "").Replace("/", "").Replace("=", "");
        var keyHash = HashKey(rawKey);
        var keyPrefix = rawKey[..11]; // "ak_" + 8 chars

        var entity = new ApiKey
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            KeyHash = keyHash,
            KeyPrefix = keyPrefix,
            Name = name,
            CreatedAt = DateTime.UtcNow
        };

        return (rawKey, entity);
    }

    public async Task<(Guid organizationId, string orgName)?> ValidateKeyAsync(string rawKey)
    {
        var keyHash = HashKey(rawKey);
        var apiKey = await _db.ApiKeys
            .Include(k => k.Organization)
            .FirstOrDefaultAsync(k => k.KeyHash == keyHash && k.IsActive && k.Organization.IsActive);

        if (apiKey is null) return null;

        apiKey.LastUsedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return (apiKey.OrganizationId, apiKey.Organization.Name);
    }

    public static string HashKey(string rawKey)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawKey));
        return Convert.ToHexStringLower(bytes);
    }
}
