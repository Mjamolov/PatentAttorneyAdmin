using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatentAttorneyAdmin.Data;
using PatentAttorneyAdmin.Infrastructure;

namespace PatentAttorneyAdmin.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? q)
    {
        var services = await _context.ServiceCategories
            .Where(s => s.IsActive)
            .OrderBy(s => s.SortOrder)
            .ToListAsync();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var isTj = LocalizationHelper.IsTajik(HttpContext);
            services = services.Where(s =>
                (isTj ? s.TitleTj : s.TitleRu).Contains(q, StringComparison.OrdinalIgnoreCase) ||
                (isTj ? s.DescriptionTj : s.DescriptionRu).Contains(q, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        ViewBag.SearchQuery = q;
        return View(services);
    }

    public IActionResult News() => View();

    public IActionResult Error() => View();
}
