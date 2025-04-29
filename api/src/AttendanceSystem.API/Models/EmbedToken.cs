namespace AttendanceSystem.API.Models;

public class EmbedToken
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SecretHash { get; set; } = string.Empty;
    public string Scopes { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public Organization Organization { get; set; } = null!;
}
