using DTS.Application.DTOs;

namespace DTS.Application.Interfaces;

public interface IProjectService
{
    Task<ProjectDto?> GetByIdAsync(int id);
    Task<IEnumerable<ProjectDto>> GetAllAsync();
    Task<IEnumerable<ProjectDto>> GetByUserAsync(string userId);
    Task<ProjectDto> CreateAsync(ProjectCreateDto dto, string userId);
    Task<ProjectDto?> UpdateAsync(ProjectUpdateDto dto);
    Task<bool> DeleteAsync(int id);
    Task<bool> AssignMemberAsync(AssignMemberDto dto);
    Task<bool> RemoveMemberAsync(int projectMemberId);
    Task<IEnumerable<ProjectMemberDto>> GetMembersAsync(int projectId);
}
