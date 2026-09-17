using DTS.Application.DTOs;

namespace DTS.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardDataAsync(string? userId = null);
    Task<List<ActivityDto>> GetRecentActivitiesAsync(int count = 10);
    Task<List<ChartDataDto>> GetDefectsByStatusAsync();
    Task<List<ChartDataDto>> GetDefectsByPriorityAsync();
    Task<List<ChartDataDto>> GetDefectsBySeverityAsync();
    Task<List<MonthlyTrendDto>> GetMonthlyTrendsAsync(int months = 12);
    Task<List<ProjectProgressDto>> GetProjectProgressAsync();
}
