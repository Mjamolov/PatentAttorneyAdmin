using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using PatentAttorneyAdmin.Data;
using PatentAttorneyAdmin.Infrastructure;
using PatentAttorneyAdmin.Models;
using PatentAttorneyAdmin.Resources;
using PatentAttorneyAdmin.Services;
using PatentAttorneyAdmin.ViewModels;

namespace PatentAttorneyAdmin.Areas.Client.Controllers;

[Area("Client")]
[Authorize(Roles = "Client")]
public class ServiceApplicationsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFileStorageService _fileStorage;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public ServiceApplicationsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IFileStorageService fileStorage, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _userManager = userManager;
        _fileStorage = fileStorage;
        _localizer = localizer;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var applications = await _context.ServiceApplications
            .Include(a => a.ServiceCategory)
            .Include(a => a.Documents)
            .Where(a => a.UserId == user.Id)
            .OrderByDescending(a => a.SubmittedAt)
            .ToListAsync();

        return View(applications);
    }

    public async Task<IActionResult> Services()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var services = await _context.ServiceCategories
            .Where(s => s.IsActive && s.ServiceCode != null && ServiceCodes.ImplementedCodes.Contains(s.ServiceCode))
            .OrderBy(s => s.SortOrder)
            .ToListAsync();

        var activeApps = await _context.ServiceApplications
            .Include(a => a.Documents)
            .Where(a => a.UserId == user.Id &&
                (a.Status == ServiceApplicationStatus.Submitted ||
                 a.Status == ServiceApplicationStatus.UnderReview ||
                 a.Status == ServiceApplicationStatus.NeedsCorrection))
            .ToListAsync();

        ViewBag.ActiveByService = activeApps.ToDictionary(a => a.ServiceCategoryId, a => a);
        ViewBag.IsTj = LocalizationHelper.IsTajik(HttpContext);
        return View(services);
    }

    [HttpGet]
    public async Task<IActionResult> Apply(int serviceId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        if (user.Status != UserStatus.Active)
        {
            TempData["Info"] = _localizer["AccountPending"].Value;
            return RedirectToAction(nameof(Services));
        }

        var service = await GetServiceAsync(serviceId);
        if (service == null) return NotFound();

        if (await HasBlockingApplicationAsync(user.Id, serviceId))
        {
            TempData["Info"] = _localizer["ApplicationAlreadyPending"].Value;
            return RedirectToAction(nameof(Index));
        }

        return View(BuildApplyView(service));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<IActionResult> Apply(int serviceId, ServiceApplicationSubmitViewModel? model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var service = await GetServiceAsync(serviceId);
        if (service == null) return NotFound();

        if (await HasBlockingApplicationAsync(user.Id, serviceId))
        {
            TempData["Info"] = _localizer["ApplicationAlreadyPending"].Value;
            return RedirectToAction(nameof(Index));
        }

        model ??= new ServiceApplicationSubmitViewModel();
        model.Documents = DocumentUploadBinder.BindDocuments(Request.Form.Files);

        var requirements = ServiceDocumentCatalog.GetRequirements(service.ServiceCode)!;
        ValidateDocuments(model, requirements);

        if (!ModelState.IsValid)
            return View(BuildApplyView(service));

        var application = await CreateApplicationAsync(user.Id, service);
        var documents = await CreateDocumentEntitiesAsync(application.Id, model, requirements);
        if (documents == null)
        {
            _context.ServiceApplications.Remove(application);
            await _context.SaveChangesAsync();
            return View(BuildApplyView(service));
        }

        _context.ServiceApplicationDocuments.AddRange(documents);
        await _context.SaveChangesAsync();

        TempData["Success"] = _localizer["ApplicationSubmitted"].Value;
        return RedirectToAction(nameof(Details), new { id = application.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Resubmit(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var application = await GetOwnedApplicationAsync(id, user.Id);
        if (application == null) return NotFound();

        if (!ApplicationCorrectionHelper.HasPendingCorrections(application))
            return RedirectToAction(nameof(Details), new { id });

        ViewBag.ApplicationId = application.Id;
        ViewBag.IsResubmit = true;
        return View("Apply", BuildResubmitView(application));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<IActionResult> Resubmit(int id, ServiceApplicationSubmitViewModel? model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var application = await _context.ServiceApplications
            .Include(a => a.ServiceCategory)
            .Include(a => a.Documents)
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == user.Id);

        if (application == null) return NotFound();

        if (!ApplicationCorrectionHelper.HasPendingCorrections(application))
            return RedirectToAction(nameof(Details), new { id });

        model ??= new ServiceApplicationSubmitViewModel();
        model.Documents = DocumentUploadBinder.BindDocuments(Request.Form.Files);

        var requirements = ServiceDocumentCatalog.GetRequirements(application.ServiceCategory.ServiceCode)!;
        var documentsToCorrect = ApplicationCorrectionHelper.GetDocumentsToCorrect(application);
        ValidateDocumentsForResubmit(model, requirements, documentsToCorrect);
        ViewBag.ApplicationId = application.Id;
        ViewBag.IsResubmit = true;

        if (!ModelState.IsValid)
            return View("Apply", BuildResubmitView(application));

        if (!await ApplyResubmitDocumentsAsync(application, model, documentsToCorrect))
            return View("Apply", BuildResubmitView(application));

        foreach (var doc in application.Documents)
            doc.NeedsCorrection = false;

        application.Status = ServiceApplicationStatus.Submitted;
        application.StaffComment = null;
        application.SubmittedAt = DateTime.UtcNow;
        application.ReviewedAt = null;
        await _context.SaveChangesAsync();

        TempData["Success"] = _localizer["ApplicationResubmitted"].Value;
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var application = await GetOwnedApplicationAsync(id, user.Id);
        if (application == null) return NotFound();

        if (application.Documents.Any(d => d.NeedsCorrection) &&
            application.Status == ServiceApplicationStatus.UnderReview)
        {
            application.Status = ServiceApplicationStatus.NeedsCorrection;
            await _context.SaveChangesAsync();
        }

        ViewBag.IsTj = LocalizationHelper.IsTajik(HttpContext);
        ViewBag.HasPendingCorrections = ApplicationCorrectionHelper.HasPendingCorrections(application);
        return View(application);
    }

    private async Task<ServiceCategory?> GetServiceAsync(int serviceId) =>
        await _context.ServiceCategories
            .FirstOrDefaultAsync(s => s.Id == serviceId && s.IsActive && s.ServiceCode != null && ServiceCodes.ImplementedCodes.Contains(s.ServiceCode));

    private async Task<ServiceApplication?> GetOwnedApplicationAsync(int id, string userId) =>
        await _context.ServiceApplications
            .Include(a => a.ServiceCategory)
            .Include(a => a.Documents)
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

    private static async Task<bool> HasBlockingApplicationAsync(ApplicationDbContext context, string userId, int serviceId) =>
        await context.ServiceApplications.AnyAsync(a =>
            a.UserId == userId &&
            a.ServiceCategoryId == serviceId &&
            (a.Status == ServiceApplicationStatus.Submitted ||
             a.Status == ServiceApplicationStatus.UnderReview));

    private async Task<bool> HasBlockingApplicationAsync(string userId, int serviceId) =>
        await HasBlockingApplicationAsync(_context, userId, serviceId);

    private object BuildApplyView(ServiceCategory service)
    {
        ViewBag.Service = service;
        ViewBag.Requirements = ServiceDocumentCatalog.GetRequirements(service.ServiceCode);
        ViewBag.DescriptionKey = ServiceDocumentCatalog.GetDescriptionKey(service.ServiceCode);
        return new ServiceApplicationSubmitViewModel();
    }

    private object BuildResubmitView(ServiceApplication application)
    {
        ViewBag.Service = application.ServiceCategory;
        ViewBag.Requirements = ServiceDocumentCatalog.GetRequirements(application.ServiceCategory.ServiceCode);
        ViewBag.DescriptionKey = ServiceDocumentCatalog.GetDescriptionKey(application.ServiceCategory.ServiceCode);
        ViewBag.ExistingDocuments = application.Documents.ToDictionary(d => (int)d.DocumentType);
        ViewBag.DocumentsToCorrect = ApplicationCorrectionHelper.GetDocumentsToCorrect(application);
        ViewBag.ApplicationStaffComment = application.StaffComment;
        return new ServiceApplicationSubmitViewModel();
    }

    private async Task<ServiceApplication> CreateApplicationAsync(string userId, ServiceCategory service)
    {
        var application = new ServiceApplication
        {
            UserId = userId,
            ServiceCategoryId = service.Id,
            Status = ServiceApplicationStatus.Submitted,
            CreatedAt = DateTime.UtcNow,
            SubmittedAt = DateTime.UtcNow
        };

        _context.ServiceApplications.Add(application);
        await _context.SaveChangesAsync();
        return application;
    }

    private async Task<List<ServiceApplicationDocument>?> CreateDocumentEntitiesAsync(
        int applicationId,
        ServiceApplicationSubmitViewModel model,
        IReadOnlyList<DocumentRequirement> requirements)
    {
        var documents = new List<ServiceApplicationDocument>();

        foreach (var req in requirements)
        {
            var key = (int)req.Type;
            if (!model.Documents.TryGetValue(key, out var file) || file == null || file.Length == 0)
                continue;

            try
            {
                var path = await _fileStorage.SaveFileAsync(file, $"applications/{applicationId}");
                documents.Add(new ServiceApplicationDocument
                {
                    ServiceApplicationId = applicationId,
                    DocumentType = req.Type,
                    FilePath = path,
                    OriginalFileName = file.FileName,
                    UploadedAt = DateTime.UtcNow
                });
            }
            catch (InvalidOperationException)
            {
                ModelState.AddModelError($"Documents[{key}]", _localizer["UnsupportedFileType"]);
                return null;
            }
            catch (ArgumentException)
            {
                ModelState.AddModelError($"Documents[{key}]", _localizer["Required"]);
                return null;
            }
        }

        return documents;
    }

    private void ValidateDocuments(ServiceApplicationSubmitViewModel model, IReadOnlyList<DocumentRequirement> requirements)
    {
        model.Documents ??= new Dictionary<int, IFormFile>();

        foreach (var req in requirements)
        {
            var key = (int)req.Type;
            if (!model.Documents.TryGetValue(key, out var file) || file == null || file.Length == 0)
                ModelState.AddModelError($"Documents[{key}]", _localizer["Required"]);
        }
    }

    private void ValidateDocumentsForResubmit(
        ServiceApplicationSubmitViewModel model,
        IReadOnlyList<DocumentRequirement> requirements,
        HashSet<int> documentsToCorrect)
    {
        model.Documents ??= new Dictionary<int, IFormFile>();

        foreach (var req in requirements)
        {
            var key = (int)req.Type;
            if (!documentsToCorrect.Contains(key))
                continue;

            if (!model.Documents.TryGetValue(key, out var file) || file == null || file.Length == 0)
                ModelState.AddModelError($"Documents[{key}]", _localizer["Required"]);
        }
    }

    private async Task<bool> ApplyResubmitDocumentsAsync(
        ServiceApplication application,
        ServiceApplicationSubmitViewModel model,
        HashSet<int> documentsToCorrect)
    {
        foreach (var key in documentsToCorrect)
        {
            if (!model.Documents.TryGetValue(key, out var file) || file == null || file.Length == 0)
                continue;

            var existing = application.Documents.FirstOrDefault(d => (int)d.DocumentType == key);
            if (existing == null)
            {
                ModelState.AddModelError($"Documents[{key}]", _localizer["Required"]);
                continue;
            }

            try
            {
                var path = await _fileStorage.SaveFileAsync(file, $"applications/{application.Id}");
                existing.FilePath = path;
                existing.OriginalFileName = file.FileName;
                existing.UploadedAt = DateTime.UtcNow;
                existing.NeedsCorrection = false;
                existing.StaffComment = null;
            }
            catch (InvalidOperationException)
            {
                ModelState.AddModelError($"Documents[{key}]", _localizer["UnsupportedFileType"]);
            }
            catch (ArgumentException)
            {
                ModelState.AddModelError($"Documents[{key}]", _localizer["Required"]);
            }
        }

        return ModelState.IsValid;
    }
}
