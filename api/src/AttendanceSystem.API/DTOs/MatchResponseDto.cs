namespace AttendanceSystem.API.DTOs;

public class MatchResponseDto
{
    public bool IsMatch { get; set; }
    public Guid? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string? PhotoUrl { get; set; }
    public double Distance { get; set; }
}
