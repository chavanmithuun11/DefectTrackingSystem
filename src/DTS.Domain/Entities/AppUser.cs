using Microsoft.AspNetCore.Identity;

namespace DTS.Domain.Entities;

public class AppUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();
    public virtual ICollection<Defect> ReportedDefects { get; set; } = new List<Defect>();
    public virtual ICollection<Defect> AssignedDefects { get; set; } = new List<Defect>();
    public virtual ICollection<DefectComment> Comments { get; set; } = new List<DefectComment>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
