using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.API.DTOs;

public class UpdateEmployeeDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(200)]
    public string? Email { get; set; }

    public string? FaceDescriptor { get; set; }
}
