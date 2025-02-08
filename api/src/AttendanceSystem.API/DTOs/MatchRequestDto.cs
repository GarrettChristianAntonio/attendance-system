using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.API.DTOs;

public class MatchRequestDto
{
    [Required]
    public float[] Descriptor { get; set; } = [];
}
