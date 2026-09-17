namespace DTS.Domain.Entities;

public class DefectAttachment : BaseEntity
{
    public int DefectId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string UploadedById { get; set; } = string.Empty;
    public string? Description { get; set; }

    public virtual Defect Defect { get; set; } = null!;
    public virtual AppUser UploadedBy { get; set; } = null!;
}
