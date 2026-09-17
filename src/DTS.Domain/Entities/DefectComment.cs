namespace DTS.Domain.Entities;

public class DefectComment : BaseEntity
{
    public int DefectId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    public virtual Defect Defect { get; set; } = null!;
    public virtual AppUser User { get; set; } = null!;
}
