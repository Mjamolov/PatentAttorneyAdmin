using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatentAttorneyAdmin.Models;

namespace PatentAttorneyAdmin.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly Data.ApplicationDbContext _context;

    public DashboardController(Data.ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        ViewBag.TotalUsers = _context.Users.Count();
        ViewBag.PendingUsers = _context.Users.Count(u => u.Status == UserStatus.Pending);
        ViewBag.TotalRequests = _context.ClientRequests.Count();
        ViewBag.OpenRequests = _context.ClientRequests.Count(r => r.Status == RequestStatus.Open || r.Status == RequestStatus.InProgress);
        ViewBag.OpenServiceApplications = _context.ServiceApplications.Count(a =>
            a.Status == ServiceApplicationStatus.Submitted || a.Status == ServiceApplicationStatus.UnderReview);
        return View();
    }
}
