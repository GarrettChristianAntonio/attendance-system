namespace AttendanceSystem.API.Models;

public class EmployeeSchedule
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid ShiftId { get; set; }
    public int DayOfWeek { get; set; } // 0=Sunday, 6=Saturday
    public DateOnly? SpecificDate { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Employee Employee { get; set; } = null!;
    public Shift Shift { get; set; } = null!;
}
