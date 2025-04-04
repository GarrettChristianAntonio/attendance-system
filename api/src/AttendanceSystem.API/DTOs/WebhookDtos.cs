using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.API.DTOs;

public class CreateWebhookDto
{
    [Required, Url]
    public string Url { get; set; } = string.Empty;

    public string Events { get; set; } = "check_in,check_out";
}

public class WebhookDto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Events { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastTriggeredAt { get; set; }
}
