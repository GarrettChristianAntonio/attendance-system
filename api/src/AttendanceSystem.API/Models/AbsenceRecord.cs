namespace AttendanceSystem.API.Models;

public class AbsenceRecord
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid ShiftId { get; set; }
    public DateOnly Date { get; set; }
    public string Type { get; set; } = "Absent"; // "Absent" or "NoCheckOut"
    public bool IsExcused { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Employee Employee { get; set; } = null!;
    public Shift Shift { get; set; } = null!;
}
