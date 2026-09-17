using DTS.Application.DTOs;
using DTS.Domain.Entities;
using DTS.Domain.Enums;
using DTS.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DTS.Web.Controllers;

[Authorize(Roles = "Administrator,ProjectManager,Tester,Developer,Client")]
public class ProjectController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;

    public ProjectController(ApplicationDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        var isAdmin = User.IsInRole("Administrator");
        var isManager = User.IsInRole("ProjectManager");

        var query = _context.Projects.Where(p => !p.IsDeleted).AsQueryable();

        if (!isAdmin && !isManager && user != null)
        {
            var userProjectIds = await _context.ProjectMembers
                .Where(pm => pm.UserId == user.Id)
                .Select(pm => pm.ProjectId)
                .ToListAsync();
            query = query.Where(p => userProjectIds.Contains(p.Id));
        }

        var projects = await query
            .Select(p => new ProjectDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Status = p.Status,
                StatusName = p.Status.ToString(),
                TotalDefects = p.Defects.Count(d => !d.IsDeleted),
                OpenDefects = p.Defects.Count(d => !d.IsDeleted && d.Status != DefectStatus.Closed),
                ClosedDefects = p.Defects.Count(d => !d.IsDeleted && d.Status == DefectStatus.Closed),
                CreatedAt = p.CreatedAt
            })
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return View(projects);
    }

    public async Task<IActionResult> Details(int id)
    {
        var project = await _context.Projects
            .Include(p => p.Members)
            .ThenInclude(m => m.User)
            .Include(p => p.Defects.Where(d => !d.IsDeleted))
            .ThenInclude(d => d.ReportedBy)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (project == null) return NotFound();

        var dto = new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            Status = project.Status,
            StatusName = project.Status.ToString(),
            TotalDefects = project.Defects.Count,
            OpenDefects = project.Defects.Count(d => d.Status != DefectStatus.Closed),
            ClosedDefects = project.Defects.Count(d => d.Status == DefectStatus.Closed),
            CreatedAt = project.CreatedAt,
            Members = project.Members.Select(m => new ProjectMemberDto
            {
                Id = m.Id,
                UserId = m.UserId,
                UserName = m.User.FullName,
                UserEmail = m.User.Email!,
                Role = m.Role
            }).ToList()
        };

        return View(dto);
    }

    [Authorize(Roles = "Administrator,ProjectManager")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,ProjectManager")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProjectCreateDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var project = new Project
        {
            Name = dto.Name,
            Description = dto.Description,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Status = dto.Status,
            CreatedAt = DateTime.UtcNow
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Project created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrator,ProjectManager")]
    public async Task<IActionResult> Edit(int id)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        if (project == null) return NotFound();

        var dto = new ProjectUpdateDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            Status = project.Status
        };

        return View(dto);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,ProjectManager")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProjectUpdateDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == dto.Id && !p.IsDeleted);
        if (project == null) return NotFound();

        project.Name = dto.Name;
        project.Description = dto.Description;
        project.StartDate = dto.StartDate;
        project.EndDate = dto.EndDate;
        project.Status = dto.Status;
        project.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Project updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,ProjectManager")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        if (project == null) return NotFound();

        project.IsDeleted = true;
        project.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Project deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrator,ProjectManager")]
    public async Task<IActionResult> AssignMember(int id)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        if (project == null) return NotFound();

        var existingMemberIds = await _context.ProjectMembers
            .Where(pm => pm.ProjectId == id)
            .Select(pm => pm.UserId)
            .ToListAsync();

        var users = await _userManager.Users
            .Where(u => u.IsActive && !existingMemberIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FullName, u.Email })
            .ToListAsync();

        ViewBag.ProjectId = id;
        ViewBag.ProjectName = project.Name;
        ViewBag.Users = users;
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,ProjectManager")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignMember(AssignMemberDto dto)
    {
        if (await _context.ProjectMembers.AnyAsync(pm => pm.ProjectId == dto.ProjectId && pm.UserId == dto.UserId))
        {
            TempData["ErrorMessage"] = "User is already a member of this project.";
            return RedirectToAction("Details", new { id = dto.ProjectId });
        }

        var member = new ProjectMember
        {
            ProjectId = dto.ProjectId,
            UserId = dto.UserId,
            Role = dto.Role,
            CreatedAt = DateTime.UtcNow
        };

        _context.ProjectMembers.Add(member);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Member assigned successfully.";
        return RedirectToAction("Details", new { id = dto.ProjectId });
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,ProjectManager")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMember(int id)
    {
        var member = await _context.ProjectMembers.FindAsync(id);
        if (member == null) return NotFound();

        _context.ProjectMembers.Remove(member);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Member removed successfully.";
        return RedirectToAction("Details", new { id = member.ProjectId });
    }
}
