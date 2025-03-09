using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.API.DTOs;

public class CreateScheduleDto
{
    [Required]
    public Guid EmployeeId { get; set; }

    [Required]
    public Guid ShiftId { get; set; }

    [Range(0, 6)]
    public int DayOfWeek { get; set; }

    public string? SpecificDate { get; set; } // "yyyy-MM-dd"

    [Required]
    public string EffectiveFrom { get; set; } = string.Empty;

    public string? EffectiveTo { get; set; }
}

public class ScheduleDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public Guid ShiftId { get; set; }
    public string ShiftName { get; set; } = string.Empty;
    public string ShiftColor { get; set; } = string.Empty;
    public int DayOfWeek { get; set; }
    public string? SpecificDate { get; set; }
    public string EffectiveFrom { get; set; } = string.Empty;
    public string? EffectiveTo { get; set; }
}

public class WeeklyScheduleDto
{
    public string WeekStart { get; set; } = string.Empty;
    public List<ScheduleEntry> Entries { get; set; } = [];
}

public class ScheduleEntry
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public Dictionary<int, List<ShiftSlot>> Days { get; set; } = new();
}

public class ShiftSlot
{
    public Guid ScheduleId { get; set; }
    public Guid ShiftId { get; set; }
    public string ShiftName { get; set; } = string.Empty;
    public string ShiftColor { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
}
