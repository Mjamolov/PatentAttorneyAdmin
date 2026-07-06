using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using PatentAttorneyAdmin.Data;
using PatentAttorneyAdmin.Infrastructure;
using PatentAttorneyAdmin.Models;
using PatentAttorneyAdmin.Resources;
using PatentAttorneyAdmin.Services;
using PatentAttorneyAdmin.ViewModels;

namespace PatentAttorneyAdmin.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ServiceApplicationsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public ServiceApplicationsController(ApplicationDbContext context, IEmailService emailService, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _emailService = emailService;
        _localizer = localizer;
    }

    public async Task<IActionResult> Index(ServiceApplicationStatus? status)
    {
        var query = _context.ServiceApplications
            .Include(a => a.User)
            .Include(a => a.ServiceCategory)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        var applications = await query
            .OrderByDescending(a => a.SubmittedAt)
            .ToListAsync();

        ViewBag.FilterStatus = status;
        ViewBag.NewCount = await _context.ServiceApplications.CountAsync(a => a.Status == ServiceApplicationStatus.Submitted);
        ViewBag.IsTj = LocalizationHelper.IsTajik(HttpContext);
        return View(applications);
    }

    public async Task<IActionResult> Details(int id)
    {
        var application = await _context.ServiceApplications
            .Include(a => a.User)
            .Include(a => a.ServiceCategory)
            .Include(a => a.Documents)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application == null) return NotFound();

        if (application.Status == ServiceApplicationStatus.Submitted)
        {
            application.Status = ServiceApplicationStatus.UnderReview;
            await _context.SaveChangesAsync();
        }

        ViewBag.IsTj = LocalizationHelper.IsTajik(HttpContext);
        return View(application);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestCorrection(RequestCorrectionViewModel model)
    {
        var application = await _context.ServiceApplications
            .Include(a => a.User)
            .Include(a => a.ServiceCategory)
            .Include(a => a.Documents)
            .FirstOrDefaultAsync(a => a.Id == model.Id);

        if (application == null) return NotFound();

        model.StaffComment = model.StaffComment?.Trim() ?? string.Empty;
        model.DocumentsToCorrect ??= new List<int>();

        if (string.IsNullOrWhiteSpace(model.StaffComment))
        {
            TempData["Error"] = _localizer["CorrectionCommentRequired"].Value;
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        if (!model.DocumentsToCorrect.Any())
        {
            TempData["Error"] = _localizer["SelectDocumentsToCorrect"].Value;
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        var selected = model.DocumentsToCorrect.ToHashSet();
        foreach (var doc in application.Documents)
        {
            var typeKey = (int)doc.DocumentType;
            doc.NeedsCorrection = selected.Contains(typeKey);
            doc.StaffComment = doc.NeedsCorrection && model.DocumentComments != null &&
                model.DocumentComments.TryGetValue(typeKey, out var comment) &&
                !string.IsNullOrWhiteSpace(comment)
                    ? comment.Trim()
                    : null;
        }

        application.Status = ServiceApplicationStatus.NeedsCorrection;
        application.StaffComment = model.StaffComment;
        application.ReviewedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await NotifyClientAsync(application, ServiceApplicationStatus.NeedsCorrection);
        TempData["Success"] = _localizer["CorrectionRequested"].Value;
        return RedirectToAction(nameof(Details), new { id = model.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(ReviewServiceApplicationViewModel model)
    {
        var application = await _context.ServiceApplications
            .Include(a => a.User)
            .Include(a => a.ServiceCategory)
            .FirstOrDefaultAsync(a => a.Id == model.Id);

        if (application == null) return NotFound();

        if (model.Status == ServiceApplicationStatus.NeedsCorrection)
        {
            TempData["Info"] = _localizer["UseCorrectionForm"].Value;
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        application.Status = model.Status;
        application.StaffComment = model.StaffComment?.Trim();
        application.ReviewedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await NotifyClientAsync(application, model.Status);
        TempData["Success"] = _localizer["ApplicationReviewed"].Value;
        return RedirectToAction(nameof(Details), new { id = model.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickReview(int id, ServiceApplicationStatus status, string? comment)
    {
        var application = await _context.ServiceApplications
            .Include(a => a.User)
            .Include(a => a.ServiceCategory)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application == null) return NotFound();

        if (status == ServiceApplicationStatus.NeedsCorrection)
        {
            TempData["Info"] = _localizer["UseCorrectionForm"].Value;
            return RedirectToAction(nameof(Details), new { id });
        }

        application.Status = status;
        application.StaffComment = comment?.Trim();
        application.ReviewedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await NotifyClientAsync(application, status);
        TempData["Success"] = _localizer["ApplicationReviewed"].Value;
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task NotifyClientAsync(ServiceApplication application, ServiceApplicationStatus status)
    {
        if (string.IsNullOrEmpty(application.User.Email)) return;

        var statusText = GetStatusText(status);
        var body = $"<p>{_localizer["ApplicationStatusChanged"]}</p>" +
                   $"<p><strong>{statusText}</strong></p>";

        if (!string.IsNullOrWhiteSpace(application.StaffComment))
            body += $"<p>{_localizer["StaffResponse"]}: {application.StaffComment}</p>";

        body += $"<p>{_localizer["ViewInCabinet"]}</p>";

        await _emailService.SendAsync(application.User.Email, _localizer["MyApplications"], body);
    }

    private string GetStatusText(ServiceApplicationStatus status) =>
        status switch
        {
            ServiceApplicationStatus.Submitted => _localizer["AppStatusSubmitted"],
            ServiceApplicationStatus.UnderReview => _localizer["AppStatusUnderReview"],
            ServiceApplicationStatus.Approved => _localizer["AppStatusApproved"],
            ServiceApplicationStatus.Rejected => _localizer["AppStatusRejected"],
            ServiceApplicationStatus.NeedsCorrection => _localizer["AppStatusNeedsCorrection"],
            _ => status.ToString()
        };
}
