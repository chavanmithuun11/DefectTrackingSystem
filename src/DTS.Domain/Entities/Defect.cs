using DTS.Domain.Enums;

namespace DTS.Domain.Entities;

public class Defect : BaseEntity
{
    public string DefectId { get; set; } = string.Empty;
    public int ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DefectSeverity Severity { get; set; } = DefectSeverity.Low;
    public DefectPriority Priority { get; set; } = DefectPriority.Low;
    public DefectStatus Status { get; set; } = DefectStatus.New;
    public string? AssignedToId { get; set; }
    public string ReportedById { get; set; } = string.Empty;
    public DateTime? ExpectedResolutionDate { get; set; }
    public DateTime? ActualResolutionDate { get; set; }
    public string? ResolutionNotes { get; set; }
    public string? StepsToReproduce { get; set; }

    public virtual Project Project { get; set; } = null!;
    public virtual AppUser? AssignedTo { get; set; }
    public virtual AppUser ReportedBy { get; set; } = null!;
    public virtual ICollection<DefectComment> Comments { get; set; } = new List<DefectComment>();
    public virtual ICollection<DefectAttachment> Attachments { get; set; } = new List<DefectAttachment>();
}
