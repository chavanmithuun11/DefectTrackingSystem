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
public class DefectController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;

    public DefectController(ApplicationDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(int? projectId, DefectStatus? status, DefectSeverity? severity, DefectPriority? priority, string? search)
    {
        var user = await _userManager.GetUserAsync(User);
        var isAdmin = User.IsInRole("Administrator");
        var isManager = User.IsInRole("ProjectManager");

        var query = _context.Defects
            .Include(d => d.Project)
            .Include(d => d.AssignedTo)
            .Include(d => d.ReportedBy)
            .Where(d => !d.IsDeleted)
            .AsQueryable();

        if (!isAdmin && !isManager && user != null)
        {
            var userProjectIds = await _context.ProjectMembers
                .Where(pm => pm.UserId == user.Id)
                .Select(pm => pm.ProjectId)
                .ToListAsync();
            query = query.Where(d => userProjectIds.Contains(d.ProjectId) || d.AssignedToId == user.Id || d.ReportedById == user.Id);
        }

        if (projectId.HasValue) query = query.Where(d => d.ProjectId == projectId.Value);
        if (status.HasValue) query = query.Where(d => d.Status == status.Value);
        if (severity.HasValue) query = query.Where(d => d.Severity == severity.Value);
        if (priority.HasValue) query = query.Where(d => d.Priority == priority.Value);
        if (!string.IsNullOrEmpty(search)) query = query.Where(d => d.Title.Contains(search) || d.DefectId.Contains(search));

        var defects = await query
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new DefectDto
            {
                Id = d.Id,
                DefectId = d.DefectId,
                ProjectId = d.ProjectId,
                ProjectName = d.Project.Name,
                Title = d.Title,
                Description = d.Description.Length > 100 ? d.Description.Substring(0, 100) + "..." : d.Description,
                Severity = d.Severity,
                SeverityName = d.Severity.ToString(),
                Priority = d.Priority,
                PriorityName = d.Priority.ToString(),
                Status = d.Status,
                StatusName = d.Status.ToString(),
                AssignedToId = d.AssignedToId,
                AssignedToName = d.AssignedTo != null ? d.AssignedTo.FullName : null,
                ReportedById = d.ReportedById,
                ReportedByName = d.ReportedBy.FullName,
                CreatedAt = d.CreatedAt,
                ExpectedResolutionDate = d.ExpectedResolutionDate,
                CommentCount = d.Comments.Count,
                AttachmentCount = d.Attachments.Count
            })
            .ToListAsync();

        ViewBag.Projects = await _context.Projects.Where(p => !p.IsDeleted).ToListAsync();
        ViewBag.Statuses = Enum.GetValues<DefectStatus>();
        ViewBag.Severities = Enum.GetValues<DefectSeverity>();
        ViewBag.Priorities = Enum.GetValues<DefectPriority>();
        ViewBag.Users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();

        return View(defects);
    }

    public async Task<IActionResult> Details(int id)
    {
        var defect = await _context.Defects
            .Include(d => d.Project)
            .Include(d => d.AssignedTo)
            .Include(d => d.ReportedBy)
            .Include(d => d.Comments)
            .ThenInclude(c => c.User)
            .Include(d => d.Attachments)
            .ThenInclude(a => a.UploadedBy)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

        if (defect == null) return NotFound();

        var currentUser = await _userManager.GetUserAsync(User);

        var dto = new DefectDto
        {
            Id = defect.Id,
            DefectId = defect.DefectId,
            ProjectId = defect.ProjectId,
            ProjectName = defect.Project.Name,
            Title = defect.Title,
            Description = defect.Description,
            Severity = defect.Severity,
            SeverityName = defect.Severity.ToString(),
            Priority = defect.Priority,
            PriorityName = defect.Priority.ToString(),
            Status = defect.Status,
            StatusName = defect.Status.ToString(),
            AssignedToId = defect.AssignedToId,
            AssignedToName = defect.AssignedTo?.FullName,
            ReportedById = defect.ReportedById,
            ReportedByName = defect.ReportedBy.FullName,
            CreatedAt = defect.CreatedAt,
            ExpectedResolutionDate = defect.ExpectedResolutionDate,
            ActualResolutionDate = defect.ActualResolutionDate,
            ResolutionNotes = defect.ResolutionNotes,
            StepsToReproduce = defect.StepsToReproduce,
            CommentCount = defect.Comments.Count,
            AttachmentCount = defect.Attachments.Count
        };

        ViewBag.Comments = defect.Comments.Select(c => new DefectCommentDto
        {
            Id = c.Id,
            DefectId = c.DefectId,
            UserId = c.UserId,
            UserName = c.User.FullName,
            Content = c.Content,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            IsOwnComment = currentUser != null && c.UserId == currentUser.Id
        }).ToList();

        ViewBag.Attachments = defect.Attachments.Select(a => new DefectAttachmentDto
        {
            Id = a.Id,
            DefectId = a.DefectId,
            FileName = a.FileName,
            OriginalFileName = a.OriginalFileName,
            ContentType = a.ContentType,
            FileSize = a.FileSize,
            UploadedByName = a.UploadedBy.FullName,
            CreatedAt = a.CreatedAt,
            Description = a.Description
        }).ToList();

        ViewBag.Developers = await _userManager.GetUsersInRoleAsync("Developer");
        ViewBag.CanEdit = currentUser != null && (User.IsInRole("Administrator") || defect.ReportedById == currentUser.Id);
        ViewBag.CanAssign = User.IsInRole("Administrator") || User.IsInRole("ProjectManager");
        ViewBag.CanChangeStatus = currentUser != null && (User.IsInRole("Administrator") || defect.AssignedToId == currentUser.Id);

        return View(dto);
    }

    [Authorize(Roles = "Administrator,ProjectManager,Tester")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Projects = await _context.Projects.Where(p => !p.IsDeleted).ToListAsync();
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,ProjectManager,Tester")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DefectCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Projects = await _context.Projects.Where(p => !p.IsDeleted).ToListAsync();
            return View(dto);
        }

        var user = await _userManager.GetUserAsync(User);
        var count = await _context.Defects.CountAsync() + 1;
        var defectId = $"BUG-{DateTime.UtcNow:yyyyMMdd}-{count:D4}";

        var defect = new Defect
        {
            DefectId = defectId,
            ProjectId = dto.ProjectId,
            Title = dto.Title,
            Description = dto.Description,
            Severity = dto.Severity,
            Priority = dto.Priority,
            Status = DefectStatus.New,
            ReportedById = user!.Id,
            StepsToReproduce = dto.StepsToReproduce,
            ExpectedResolutionDate = dto.ExpectedResolutionDate,
            CreatedAt = DateTime.UtcNow
        };

        _context.Defects.Add(defect);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Defect '{defectId}' created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Administrator,ProjectManager,Tester")]
    public async Task<IActionResult> Edit(int id)
    {
        var defect = await _context.Defects.FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
        if (defect == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (!User.IsInRole("Administrator") && defect.ReportedById != user?.Id)
        {
            return Forbid();
        }

        var dto = new DefectUpdateDto
        {
            Id = defect.Id,
            Title = defect.Title,
            Description = defect.Description,
            Severity = defect.Severity,
            Priority = defect.Priority,
            StepsToReproduce = defect.StepsToReproduce,
            ExpectedResolutionDate = defect.ExpectedResolutionDate
        };

        ViewBag.Projects = await _context.Projects.Where(p => !p.IsDeleted).ToListAsync();
        return View(dto);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,ProjectManager,Tester")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(DefectUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Projects = await _context.Projects.Where(p => !p.IsDeleted).ToListAsync();
            return View(dto);
        }

        var defect = await _context.Defects.FirstOrDefaultAsync(d => d.Id == dto.Id && !d.IsDeleted);
        if (defect == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (!User.IsInRole("Administrator") && defect.ReportedById != user?.Id)
        {
            return Forbid();
        }

        defect.Title = dto.Title;
        defect.Description = dto.Description;
        defect.Severity = dto.Severity;
        defect.Priority = dto.Priority;
        defect.StepsToReproduce = dto.StepsToReproduce;
        defect.ExpectedResolutionDate = dto.ExpectedResolutionDate;
        defect.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Defect updated successfully.";
        return RedirectToAction(nameof(Details), new { id = defect.Id });
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,ProjectManager,Tester")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var defect = await _context.Defects.FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
        if (defect == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (!User.IsInRole("Administrator") && defect.ReportedById != user?.Id)
        {
            return Forbid();
        }

        defect.IsDeleted = true;
        defect.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Defect deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,ProjectManager")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(AssignDefectDto dto)
    {
        var defect = await _context.Defects.FirstOrDefaultAsync(d => d.Id == dto.DefectId && !d.IsDeleted);
        if (defect == null) return NotFound();

        defect.AssignedToId = dto.AssignedToId;
        defect.Status = dto.AssignedToId != null ? DefectStatus.Assigned : DefectStatus.New;
        defect.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Defect assigned successfully.";
        return RedirectToAction(nameof(Details), new { id = dto.DefectId });
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,ProjectManager,Developer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(ChangeDefectStatusDto dto)
    {
        var defect = await _context.Defects.FirstOrDefaultAsync(d => d.Id == dto.DefectId && !d.IsDeleted);
        if (defect == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (!User.IsInRole("Administrator") && !User.IsInRole("ProjectManager") && defect.AssignedToId != user?.Id)
        {
            return Forbid();
        }

        defect.Status = dto.Status;
        defect.ResolutionNotes = dto.ResolutionNotes;
        if (dto.Status == DefectStatus.Closed)
        {
            defect.ActualResolutionDate = DateTime.UtcNow;
        }
        defect.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Status updated successfully.";
        return RedirectToAction(nameof(Details), new { id = dto.DefectId });
    }
}
