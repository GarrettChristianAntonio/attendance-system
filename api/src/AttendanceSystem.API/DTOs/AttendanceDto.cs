namespace AttendanceSystem.API.DTOs;

public class AttendanceDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public DateTime CheckInAt { get; set; }
    public DateTime? CheckOutAt { get; set; }
    public double Confidence { get; set; }
    public string Status { get; set; } = "OnTime";
    public string? ShiftName { get; set; }
    public string? ShiftColor { get; set; }
}
