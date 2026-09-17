namespace DTS.Application.DTOs;

public class DefectCommentDto
{
    public int Id { get; set; }
    public int DefectId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsOwnComment { get; set; }
}

public class DefectCommentCreateDto
{
    public int DefectId { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class DefectCommentUpdateDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
}
