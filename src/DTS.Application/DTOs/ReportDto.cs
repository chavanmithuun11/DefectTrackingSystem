using DTS.Domain.Enums;

namespace DTS.Application.DTOs;

public class ReportFilterDto
{
    public int? ProjectId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DefectStatus? Status { get; set; }
    public DefectSeverity? Severity { get; set; }
    public string? AssignedToId { get; set; }
}

public class DefectSummaryReportDto
{
    public int TotalDefects { get; set; }
    public int OpenDefects { get; set; }
    public int ClosedDefects { get; set; }
    public int CriticalDefects { get; set; }
    public int HighPriorityDefects { get; set; }
    public double AverageResolutionDays { get; set; }
    public List<StatusSummaryDto> StatusSummary { get; set; } = new();
    public List<SeveritySummaryDto> SeveritySummary { get; set; } = new();
}

public class StatusSummaryDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class SeveritySummaryDto
{
    public string Severity { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class UserPerformanceDto
{
    public string UserName { get; set; } = string.Empty;
    public int TotalAssigned { get; set; }
    public int Resolved { get; set; }
    public int InProgress { get; set; }
    public int Open { get; set; }
    public double ResolutionRate { get; set; }
}

public class ProjectWiseReportDto
{
    public string ProjectName { get; set; } = string.Empty;
    public int TotalDefects { get; set; }
    public int OpenDefects { get; set; }
    public int ClosedDefects { get; set; }
    public int CriticalDefects { get; set; }
    public double ProgressPercentage { get; set; }
}
