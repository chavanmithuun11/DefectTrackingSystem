using DTS.Application.DTOs;
using DTS.Domain.Entities;
using DTS.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DTS.Web.Controllers;

[Authorize]
public class DefectCommentController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;

    public DefectCommentController(ApplicationDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DefectCommentCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Content))
        {
            TempData["ErrorMessage"] = "Comment content is required.";
            return RedirectToAction("Details", "Defect", new { id = dto.DefectId });
        }

        var user = await _userManager.GetUserAsync(User);
        var comment = new DefectComment
        {
            DefectId = dto.DefectId,
            UserId = user!.Id,
            Content = dto.Content,
            CreatedAt = DateTime.UtcNow
        };

        _context.DefectComments.Add(comment);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Comment added successfully.";
        return RedirectToAction("Details", "Defect", new { id = dto.DefectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(DefectCommentUpdateDto dto)
    {
        var comment = await _context.DefectComments.FindAsync(dto.Id);
        if (comment == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (!User.IsInRole("Administrator") && comment.UserId != user?.Id)
        {
            return Forbid();
        }

        comment.Content = dto.Content;
        comment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Comment updated successfully.";
        return RedirectToAction("Details", "Defect", new { id = comment.DefectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var comment = await _context.DefectComments.FindAsync(id);
        if (comment == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (!User.IsInRole("Administrator") && comment.UserId != user?.Id)
        {
            return Forbid();
        }

        var defectId = comment.DefectId;
        _context.DefectComments.Remove(comment);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Comment deleted successfully.";
        return RedirectToAction("Details", "Defect", new { id = defectId });
    }
}
