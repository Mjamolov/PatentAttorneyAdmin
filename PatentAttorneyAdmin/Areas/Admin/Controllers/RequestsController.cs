using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatentAttorneyAdmin.Data;
using PatentAttorneyAdmin.Models;
using PatentAttorneyAdmin.Services;
using PatentAttorneyAdmin.ViewModels;

namespace PatentAttorneyAdmin.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
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
        var requests = await _context.ClientRequests
            .Include(r => r.User)
            .Include(r => r.ServiceCategory)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
        return View(requests);
    }

    public async Task<IActionResult> Details(int id)
    {
        var request = await _context.ClientRequests
            .Include(r => r.User)
            .Include(r => r.ServiceCategory)
            .Include(r => r.Messages.OrderBy(m => m.SentAt))
                .ThenInclude(m => m.SenderUser)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (request == null) return NotFound();
        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reply(ReplyRequestViewModel model)
    {
        var request = await _context.ClientRequests.FindAsync(model.RequestId);
        if (request == null) return NotFound();

        if (!ModelState.IsValid)
            return RedirectToAction(nameof(Details), new { id = model.RequestId });

        var admin = await _userManager.GetUserAsync(User);
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
            SenderUserId = admin?.Id,
            IsFromStaff = true,
            Body = model.Body,
            AttachmentPath = attachmentPath,
            AttachmentFileName = attachmentName,
            SentAt = DateTime.UtcNow
        });

        request.Status = RequestStatus.AwaitingClient;
        request.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = request.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(int id, RequestStatus status)
    {
        var request = await _context.ClientRequests.FindAsync(id);
        if (request == null) return NotFound();

        request.Status = status;
        request.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }
}
