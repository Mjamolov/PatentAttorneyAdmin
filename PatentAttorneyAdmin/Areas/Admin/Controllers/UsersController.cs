using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatentAttorneyAdmin.Data;
using PatentAttorneyAdmin.Models;

namespace PatentAttorneyAdmin.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _context.Users
            .OrderByDescending(u => u.RegisteredAt)
            .ToListAsync();
        return View(users);
    }

    public async Task<IActionResult> Details(string id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(string id, UserStatus status)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        if (await _userManager.IsInRoleAsync(user, "Admin"))
            return Forbid();

        user.Status = status;
        await _userManager.UpdateAsync(user);
        return RedirectToAction(nameof(Details), new { id });
    }
}
