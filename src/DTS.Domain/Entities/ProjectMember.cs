namespace DTS.Domain.Entities;

public class ProjectMember : BaseEntity
{
    public int ProjectId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    public virtual Project Project { get; set; } = null!;
    public virtual AppUser User { get; set; } = null!;
}
