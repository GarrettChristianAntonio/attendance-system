namespace AttendanceSystem.API.Models;

public class WebhookEndpoint
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public string Events { get; set; } = "check_in,check_out"; // comma-separated
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastTriggeredAt { get; set; }

    public Organization Organization { get; set; } = null!;
}
