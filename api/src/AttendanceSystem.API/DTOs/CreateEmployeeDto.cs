using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.API.DTOs;

public class CreateEmployeeDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(200)]
    public string? Email { get; set; }

    [Required]
    public string FaceDescriptor { get; set; } = string.Empty;
}
