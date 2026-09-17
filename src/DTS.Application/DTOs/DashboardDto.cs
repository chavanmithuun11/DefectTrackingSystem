namespace DTS.Application.DTOs;

public class DashboardDto
{
    public int TotalProjects { get; set; }
    public int TotalDefects { get; set; }
    public int OpenDefects { get; set; }
    public int ClosedDefects { get; set; }
    public int CriticalDefects { get; set; }
    public int AssignedDefects { get; set; }
    public int PendingDefects { get; set; }
    public List<ActivityDto> RecentActivities { get; set; } = new();
    public List<ChartDataDto> DefectsByStatus { get; set; } = new();
    public List<ChartDataDto> DefectsByPriority { get; set; } = new();
    public List<ChartDataDto> DefectsBySeverity { get; set; } = new();
    public List<MonthlyTrendDto> MonthlyTrends { get; set; } = new();
    public List<ProjectProgressDto> ProjectProgress { get; set; } = new();
}

public class ActivityDto
{
    public string Description { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}

public class ChartDataDto
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
}

public class MonthlyTrendDto
{
    public string Month { get; set; } = string.Empty;
    public int Created { get; set; }
    public int Closed { get; set; }
}

public class ProjectProgressDto
{
    public string ProjectName { get; set; } = string.Empty;
    public int TotalDefects { get; set; }
    public int ResolvedDefects { get; set; }
    public int ProgressPercentage { get; set; }
}
