using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatentAttorneyAdmin.Data;
using PatentAttorneyAdmin.Models;
using PatentAttorneyAdmin.ViewModels;

namespace PatentAttorneyAdmin.Areas.Client.Controllers;

[Area("Client")]
[Authorize(Roles = "Client")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var model = new ClientDashboardViewModel
        {
            RecentRequests = await _context.ClientRequests
                .Where(r => r.UserId == user.Id)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToListAsync(),
            RecentApplications = await _context.ServiceApplications
                .Include(a => a.ServiceCategory)
                .Where(a => a.UserId == user.Id)
                .OrderByDescending(a => a.SubmittedAt)
                .Take(5)
                .ToListAsync()
        };

        return View(model);
    }
}
