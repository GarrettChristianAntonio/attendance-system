namespace AttendanceSystem.API.Models;

public class AttendanceRecord
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid? ShiftId { get; set; }
    public DateTime CheckInAt { get; set; } = DateTime.UtcNow;
    public DateTime? CheckOutAt { get; set; }
    public double Confidence { get; set; }
    public string Status { get; set; } = "OnTime"; // "OnTime", "Late", "EarlyDeparture", "Absent"
    public string? Notes { get; set; }

    public Employee Employee { get; set; } = null!;
    public Shift? Shift { get; set; }
}
