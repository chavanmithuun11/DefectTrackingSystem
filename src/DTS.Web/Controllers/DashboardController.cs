using DTS.Application.DTOs;
using DTS.Domain.Entities;
using DTS.Domain.Enums;
using DTS.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DTS.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;

    public DashboardController(ApplicationDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        var isAdmin = User.IsInRole("Administrator");
        var isManager = User.IsInRole("ProjectManager");

        var projectsQuery = _context.Projects.Where(p => !p.IsDeleted);
        var defectsQuery = _context.Defects.Where(d => !d.IsDeleted);

        if (!isAdmin && !isManager && user != null)
        {
            var userProjectIds = await _context.ProjectMembers
                .Where(pm => pm.UserId == user.Id)
                .Select(pm => pm.ProjectId)
                .ToListAsync();
            projectsQuery = projectsQuery.Where(p => userProjectIds.Contains(p.Id));
            defectsQuery = defectsQuery.Where(d => userProjectIds.Contains(d.ProjectId) || d.AssignedToId == user.Id || d.ReportedById == user.Id);
        }

        var totalProjects = await projectsQuery.CountAsync();
        var totalDefects = await defectsQuery.CountAsync();
        var openDefects = await defectsQuery.CountAsync(d => d.Status != DefectStatus.Closed);
        var closedDefects = await defectsQuery.CountAsync(d => d.Status == DefectStatus.Closed);
        var criticalDefects = await defectsQuery.CountAsync(d => d.Severity == DefectSeverity.Critical);
        var assignedDefects = await defectsQuery.CountAsync(d => d.Status == DefectStatus.Assigned || d.Status == DefectStatus.InProgress);
        var pendingDefects = await defectsQuery.CountAsync(d => d.Status == DefectStatus.New);

        var defectsByStatus = await defectsQuery
            .GroupBy(d => d.Status)
            .Select(g => new ChartDataDto { Label = g.Key.ToString(), Value = g.Count() })
            .ToListAsync();

        var defectsByPriority = await defectsQuery
            .GroupBy(d => d.Priority)
            .Select(g => new ChartDataDto { Label = g.Key.ToString(), Value = g.Count() })
            .ToListAsync();

        var defectsBySeverity = await defectsQuery
            .GroupBy(d => d.Severity)
            .Select(g => new ChartDataDto { Label = g.Key.ToString(), Value = g.Count() })
            .ToListAsync();

        var monthlyTrends = await defectsQuery
            .GroupBy(d => new { d.CreatedAt.Year, d.CreatedAt.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Created = g.Count(),
                Closed = g.Count(d => d.Status == DefectStatus.Closed)
            })
            .OrderByDescending(x => x.Year).ThenByDescending(x => x.Month)
            .Take(12)
            .ToListAsync();

        var projectProgress = await projectsQuery
            .Select(p => new ProjectProgressDto
            {
                ProjectName = p.Name,
                TotalDefects = p.Defects.Count(d => !d.IsDeleted),
                ResolvedDefects = p.Defects.Count(d => !d.IsDeleted && d.Status == DefectStatus.Closed),
                ProgressPercentage = p.Defects.Any(d => !d.IsDeleted)
                    ? (int)((double)p.Defects.Count(d => !d.IsDeleted && d.Status == DefectStatus.Closed) / p.Defects.Count(d => !d.IsDeleted) * 100)
                    : 0
            })
            .ToListAsync();

        var recentDefects = await defectsQuery
            .OrderByDescending(d => d.CreatedAt)
            .Take(5)
            .Select(d => new ActivityDto
            {
                Description = $"Defect '{d.Title}' created",
                UserName = d.ReportedBy.FullName,
                Timestamp = d.CreatedAt,
                Icon = "fa-bug",
                Color = "danger"
            })
            .ToListAsync();

        var dashboard = new DashboardDto
        {
            TotalProjects = totalProjects,
            TotalDefects = totalDefects,
            OpenDefects = openDefects,
            ClosedDefects = closedDefects,
            CriticalDefects = criticalDefects,
            AssignedDefects = assignedDefects,
            PendingDefects = pendingDefects,
            RecentActivities = recentDefects,
            DefectsByStatus = defectsByStatus,
            DefectsByPriority = defectsByPriority,
            DefectsBySeverity = defectsBySeverity,
            MonthlyTrends = monthlyTrends.Select(m => new MonthlyTrendDto
            {
                Month = $"{m.Year}-{m.Month:D2}",
                Created = m.Created,
                Closed = m.Closed
            }).ToList(),
            ProjectProgress = projectProgress
        };

        return View(dashboard);
    }
}
