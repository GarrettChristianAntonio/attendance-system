namespace AttendanceSystem.API.Models;

public class AttendanceRecord
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime CheckInAt { get; set; } = DateTime.UtcNow;
    public double Confidence { get; set; }

    public Employee Employee { get; set; } = null!;
}
