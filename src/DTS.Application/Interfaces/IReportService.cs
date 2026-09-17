using DTS.Application.DTOs;

namespace DTS.Application.Interfaces;

public interface IReportService
{
    Task<DefectSummaryReportDto> GetDefectSummaryReportAsync(ReportFilterDto? filter = null);
    Task<List<ProjectWiseReportDto>> GetProjectWiseReportAsync(ReportFilterDto? filter = null);
    Task<List<UserPerformanceDto>> GetUserPerformanceReportAsync(ReportFilterDto? filter = null);
    Task<byte[]> ExportToCsvAsync<T>(List<T> data) where T : class;
    Task<byte[]> ExportToExcelAsync<T>(List<T> data, string sheetName) where T : class;
    Task<byte[]> ExportToPdfAsync<T>(List<T> data, string title) where T : class;
}
