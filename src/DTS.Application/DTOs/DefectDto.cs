using DTS.Domain.Enums;

namespace DTS.Application.DTOs;

public class DefectDto
{
    public int Id { get; set; }
    public string DefectId { get; set; } = string.Empty;
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DefectSeverity Severity { get; set; }
    public string SeverityName { get; set; } = string.Empty;
    public DefectPriority Priority { get; set; }
    public string PriorityName { get; set; } = string.Empty;
    public DefectStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
    public string ReportedById { get; set; } = string.Empty;
    public string ReportedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpectedResolutionDate { get; set; }
    public DateTime? ActualResolutionDate { get; set; }
    public string? ResolutionNotes { get; set; }
    public string? StepsToReproduce { get; set; }
    public int CommentCount { get; set; }
    public int AttachmentCount { get; set; }
}

public class DefectCreateDto
{
    public int ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DefectSeverity Severity { get; set; } = DefectSeverity.Low;
    public DefectPriority Priority { get; set; } = DefectPriority.Low;
    public string? StepsToReproduce { get; set; }
    public DateTime? ExpectedResolutionDate { get; set; }
}

public class DefectUpdateDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DefectSeverity Severity { get; set; }
    public DefectPriority Priority { get; set; }
    public string? StepsToReproduce { get; set; }
    public DateTime? ExpectedResolutionDate { get; set; }
}

public class AssignDefectDto
{
    public int DefectId { get; set; }
    public string? AssignedToId { get; set; }
}

public class ChangeDefectStatusDto
{
    public int DefectId { get; set; }
    public DefectStatus Status { get; set; }
    public string? ResolutionNotes { get; set; }
}

public class DefectFilterDto
{
    public int? ProjectId { get; set; }
    public DefectStatus? Status { get; set; }
    public DefectSeverity? Severity { get; set; }
    public DefectPriority? Priority { get; set; }
    public string? AssignedToId { get; set; }
    public string? SearchTerm { get; set; }
}
