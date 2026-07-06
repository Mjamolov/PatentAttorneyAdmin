using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PatentAttorneyAdmin.Data;
using PatentAttorneyAdmin.Models;
using PatentAttorneyAdmin.Services;
using PatentAttorneyAdmin.ViewModels;

namespace PatentAttorneyAdmin.Areas.Client.Controllers;

[Area("Client")]
[Authorize(Roles = "Client")]
public class RequestsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFileStorageService _fileStorage;

    public RequestsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IFileStorageService fileStorage)
    {
        _context = context;
        _userManager = userManager;
        _fileStorage = fileStorage;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var requests = await _context.ClientRequests
            .Include(r => r.ServiceCategory)
            .Where(r => r.UserId == user.Id)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return View(requests);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? serviceId = null)
    {
        await LoadServiceCategories(serviceId);
        return View(new CreateRequestViewModel { ServiceCategoryId = serviceId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRequestViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadServiceCategories(model.ServiceCategoryId);
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        string? attachmentPath = null;
        string? attachmentName = null;
        if (model.Attachment != null)
        {
            attachmentPath = await _fileStorage.SaveFileAsync(model.Attachment, "requests");
            attachmentName = model.Attachment.FileName;
        }

        var request = new ClientRequest
        {
            UserId = user.Id,
            ServiceCategoryId = model.ServiceCategoryId,
            Subject = model.Subject,
            Type = model.Type,
            Status = RequestStatus.Open,
            CreatedAt = DateTime.UtcNow
        };

        _context.ClientRequests.Add(request);
        await _context.SaveChangesAsync();

        _context.RequestMessages.Add(new RequestMessage
        {
            ClientRequestId = request.Id,
            SenderUserId = user.Id,
            IsFromStaff = false,
            Body = model.Body,
            AttachmentPath = attachmentPath,
            AttachmentFileName = attachmentName,
            SentAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = request.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var request = await _context.ClientRequests
            .Include(r => r.ServiceCategory)
            .Include(r => r.Messages.OrderBy(m => m.SentAt))
                .ThenInclude(m => m.SenderUser)
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == user.Id);

        if (request == null) return NotFound();
        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reply(ReplyRequestViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var request = await _context.ClientRequests
            .FirstOrDefaultAsync(r => r.Id == model.RequestId && r.UserId == user.Id);

        if (request == null) return NotFound();

        if (!ModelState.IsValid)
            return RedirectToAction(nameof(Details), new { id = model.RequestId });

        string? attachmentPath = null;
        string? attachmentName = null;
        if (model.Attachment != null)
        {
            attachmentPath = await _fileStorage.SaveFileAsync(model.Attachment, "requests");
            attachmentName = model.Attachment.FileName;
        }

        _context.RequestMessages.Add(new RequestMessage
        {
            ClientRequestId = request.Id,
            SenderUserId = user.Id,
            IsFromStaff = false,
            Body = model.Body,
            AttachmentPath = attachmentPath,
            AttachmentFileName = attachmentName,
            SentAt = DateTime.UtcNow
        });

        request.Status = RequestStatus.InProgress;
        request.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = request.Id });
    }

    private async Task LoadServiceCategories(int? selectedId)
    {
        var services = await _context.ServiceCategories
            .Where(s => s.IsActive)
            .OrderBy(s => s.SortOrder)
            .ToListAsync();

        ViewBag.Services = new SelectList(services, "Id", "TitleRu", selectedId);
    }
}
