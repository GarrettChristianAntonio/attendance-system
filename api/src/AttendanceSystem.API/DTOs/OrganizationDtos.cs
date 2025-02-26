using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.API.DTOs;

public class CreateOrganizationDto
{
    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(100), RegularExpression(@"^[a-z0-9-]+$")]
    public string Slug { get; set; } = string.Empty;
}

public class OrganizationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateOrganizationResponseDto
{
    public OrganizationDto Organization { get; set; } = null!;
    public string ApiKey { get; set; } = string.Empty;
}
