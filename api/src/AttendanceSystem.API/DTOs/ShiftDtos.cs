using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.API.DTOs;

public class CreateShiftDto
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string StartTime { get; set; } = string.Empty; // "HH:mm"

    [Required]
    public string EndTime { get; set; } = string.Empty;

    public int GracePeriodMinutes { get; set; } = 15;

    [StringLength(7)]
    public string Color { get; set; } = "#3B82F6";
}

public class ShiftDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public int GracePeriodMinutes { get; set; }
    public string Color { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
