using System.Text;
using DTS.Application.DTOs;
using DTS.Domain.Enums;
using DTS.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DTS.Web.Controllers;

[Authorize(Roles = "Administrator,ProjectManager")]
public class ReportController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<Domain.Entities.AppUser> _userManager;

    public ReportController(ApplicationDbContext context, UserManager<Domain.Entities.AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Projects = await _context.Projects.Where(p => !p.IsDeleted).ToListAsync();
        return View();
    }

    public async Task<IActionResult> DefectSummary(ReportFilterDto? filter)
    {
        var query = _context.Defects.Where(d => !d.IsDeleted).AsQueryable();
        if (filter?.ProjectId.HasValue == true) query = query.Where(d => d.ProjectId == filter.ProjectId.Value);
        if (filter?.StartDate.HasValue == true) query = query.Where(d => d.CreatedAt >= filter.StartDate.Value);
        if (filter?.EndDate.HasValue == true) query = query.Where(d => d.CreatedAt <= filter.EndDate.Value);
        if (filter?.Status.HasValue == true) query = query.Where(d => d.Status == filter.Status.Value);
        if (filter?.Severity.HasValue == true) query = query.Where(d => d.Severity == filter.Severity.Value);

        var totalDefects = await query.CountAsync();
        var openDefects = await query.CountAsync(d => d.Status != DefectStatus.Closed);
        var closedDefects = await query.CountAsync(d => d.Status == DefectStatus.Closed);
        var criticalDefects = await query.CountAsync(d => d.Severity == DefectSeverity.Critical);
        var highPriorityDefects = await query.CountAsync(d => d.Priority == DefectPriority.High || d.Priority == DefectPriority.Urgent);

        var closedWithDates = await query
            .Where(d => d.Status == DefectStatus.Closed && d.ActualResolutionDate.HasValue)
            .Select(d => new { d.CreatedAt, d.ActualResolutionDate })
            .ToListAsync();

        var avgResolutionDays = closedWithDates.Any() ? closedWithDates.Average(d => (d.ActualResolutionDate!.Value - d.CreatedAt).TotalDays) : 0;

        var statusSummary = await query
            .GroupBy(d => d.Status)
            .Select(g => new StatusSummaryDto { Status = g.Key.ToString(), Count = g.Count() })
            .ToListAsync();

        var severitySummary = await query
            .GroupBy(d => d.Severity)
            .Select(g => new SeveritySummaryDto { Severity = g.Key.ToString(), Count = g.Count() })
            .ToListAsync();

        var report = new DefectSummaryReportDto
        {
            TotalDefects = totalDefects,
            OpenDefects = openDefects,
            ClosedDefects = closedDefects,
            CriticalDefects = criticalDefects,
            HighPriorityDefects = highPriorityDefects,
            AverageResolutionDays = avgResolutionDays,
            StatusSummary = statusSummary,
            SeveritySummary = severitySummary
        };

        return View(report);
    }

    public async Task<IActionResult> ProjectWise(ReportFilterDto? filter)
    {
        var query = _context.Projects.Where(p => !p.IsDeleted).AsQueryable();

        var reports = await query
            .Select(p => new ProjectWiseReportDto
            {
                ProjectName = p.Name,
                TotalDefects = p.Defects.Count(d => !d.IsDeleted),
                OpenDefects = p.Defects.Count(d => !d.IsDeleted && d.Status != DefectStatus.Closed),
                ClosedDefects = p.Defects.Count(d => !d.IsDeleted && d.Status == DefectStatus.Closed),
                CriticalDefects = p.Defects.Count(d => !d.IsDeleted && d.Severity == DefectSeverity.Critical),
                ProgressPercentage = p.Defects.Any(d => !d.IsDeleted)
                    ? (int)((double)p.Defects.Count(d => !d.IsDeleted && d.Status == DefectStatus.Closed) / p.Defects.Count(d => !d.IsDeleted) * 100)
                    : 0
            })
            .ToListAsync();

        return View(reports);
    }

    public async Task<IActionResult> UserPerformance(ReportFilterDto? filter)
    {
        var developers = await _userManager.GetUsersInRoleAsync("Developer");
        var reports = new List<UserPerformanceDto>();

        foreach (var dev in developers)
        {
            var assigned = await _context.Defects.CountAsync(d => !d.IsDeleted && d.AssignedToId == dev.Id);
            var resolved = await _context.Defects.CountAsync(d => !d.IsDeleted && d.AssignedToId == dev.Id && d.Status == DefectStatus.Closed);
            var inProgress = await _context.Defects.CountAsync(d => !d.IsDeleted && d.AssignedToId == dev.Id && d.Status == DefectStatus.InProgress);
            var open = await _context.Defects.CountAsync(d => !d.IsDeleted && d.AssignedToId == dev.Id && d.Status == DefectStatus.New);

            reports.Add(new UserPerformanceDto
            {
                UserName = dev.FullName,
                TotalAssigned = assigned,
                Resolved = resolved,
                InProgress = inProgress,
                Open = open,
                ResolutionRate = assigned > 0 ? (double)resolved / assigned * 100 : 0
            });
        }

        return View(reports.OrderByDescending(r => r.ResolutionRate).ToList());
    }

    public async Task<IActionResult> ExportCsv(string type)
    {
        var csv = new StringBuilder();
        csv.AppendLine("Report Type,Generated At");
        csv.AppendLine($"{type},{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
        csv.AppendLine();

        if (type == "defects")
        {
            var defects = await _context.Defects
                .Include(d => d.Project)
                .Where(d => !d.IsDeleted)
                .ToListAsync();

            csv.AppendLine("Defect ID,Title,Project,Severity,Priority,Status,Assigned To,Reported By,Created Date");
            foreach (var d in defects)
            {
                csv.AppendLine($"{d.DefectId},{d.Title},{d.Project.Name},{d.Severity},{d.Priority},{d.Status},{d.AssignedToId},{d.ReportedById},{d.CreatedAt:yyyy-MM-dd}");
            }
        }
        else if (type == "projects")
        {
            var projects = await _context.Projects.Where(p => !p.IsDeleted).ToListAsync();
            csv.AppendLine("Project Name,Status,Start Date,End Date,Total Defects");
            foreach (var p in projects)
            {
                csv.AppendLine($"{p.Name},{p.Status},{p.StartDate:yyyy-MM-dd},{p.EndDate:yyyy-MM-dd},{p.Defects.Count(d => !d.IsDeleted)}");
            }
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", $"{type}_report_{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}
