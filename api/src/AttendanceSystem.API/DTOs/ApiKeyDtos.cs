using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.API.DTOs;

public class CreateApiKeyDto
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;
}

public class ApiKeyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string KeyPrefix { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateApiKeyResponseDto
{
    public ApiKeyDto Key { get; set; } = null!;
    public string RawKey { get; set; } = string.Empty;
}
