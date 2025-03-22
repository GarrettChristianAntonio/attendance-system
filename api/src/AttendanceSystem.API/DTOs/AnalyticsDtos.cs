namespace AttendanceSystem.API.DTOs;

public class DashboardDto
{
    public int TotalEmployees { get; set; }
    public int TodayCheckIns { get; set; }
    public int OnTimeToday { get; set; }
    public int LateToday { get; set; }
    public double OnTimeRate { get; set; }
    public List<DailyStatsDto> WeeklyStats { get; set; } = [];
    public List<TopEmployeeDto> TopEmployees { get; set; } = [];
}

public class DailyStatsDto
{
    public string Date { get; set; } = string.Empty;
    public string DayLabel { get; set; } = string.Empty;
    public int CheckIns { get; set; }
    public int OnTime { get; set; }
    public int Late { get; set; }
    public int Absent { get; set; }
}

public class TopEmployeeDto
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public int OnTimeCount { get; set; }
    public int TotalDays { get; set; }
    public double OnTimeRate { get; set; }
}

public class EmployeeAnalyticsDto
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public int TotalDays { get; set; }
    public int OnTimeCount { get; set; }
    public int LateCount { get; set; }
    public int AbsentCount { get; set; }
    public int EarlyDepartureCount { get; set; }
    public double TotalHours { get; set; }
    public string? AvgArrivalTime { get; set; }
    public List<DailyStatsDto> DailyHistory { get; set; } = [];
}
