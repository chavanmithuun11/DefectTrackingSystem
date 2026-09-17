using DTS.Application.DTOs;

namespace DTS.Application.Interfaces;

public interface IDefectService
{
    Task<DefectDto?> GetByIdAsync(int id);
    Task<IEnumerable<DefectDto>> GetAllAsync(DefectFilterDto? filter = null);
    Task<IEnumerable<DefectDto>> GetByProjectAsync(int projectId);
    Task<IEnumerable<DefectDto>> GetByUserAsync(string userId);
    Task<IEnumerable<DefectDto>> GetAssignedToAsync(string userId);
    Task<DefectDto> CreateAsync(DefectCreateDto dto, string reportedById);
    Task<DefectDto?> UpdateAsync(DefectUpdateDto dto);
    Task<bool> DeleteAsync(int id);
    Task<bool> AssignAsync(AssignDefectDto dto);
    Task<bool> ChangeStatusAsync(ChangeDefectStatusDto dto, string userId);
    Task<string> GenerateDefectIdAsync();
}
