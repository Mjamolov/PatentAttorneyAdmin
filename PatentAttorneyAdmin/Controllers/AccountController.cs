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

namespace PatentAttorneyAdmin.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IEmailService _emailService;
    private readonly IFileStorageService _fileStorage;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public AccountController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IEmailService emailService,
        IFileStorageService fileStorage,
        IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
        _fileStorage = fileStorage;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> Register(int? serviceId = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToCabinet();

        await LoadRegisterViewData(serviceId);
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> Register(RegisterViewModel model, int? serviceId = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToCabinet();

        ValidatePassportFiles(model);

        if (!ModelState.IsValid)
        {
            await LoadRegisterViewData(serviceId);
            return View(model);
        }

        if (await _userManager.FindByEmailAsync(model.Email) != null)
        {
            ModelState.AddModelError(nameof(model.Email), _localizer["EmailExists"]);
            await LoadRegisterViewData(serviceId);
            return View(model);
        }

        try
        {
            var frontPath = await _fileStorage.SaveFileAsync(model.PassportFront!, "passports");
            var backPath = await _fileStorage.SaveFileAsync(model.PassportBack!, "passports");

            var password = PasswordGenerator.Generate();
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,
                PhoneNumber = model.Phone.Trim(),
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                MiddleName = model.MiddleName?.Trim(),
                PassportSeries = model.PassportSeries.Trim(),
                PassportNumber = model.PassportNumber.Trim(),
                PassportIssuedBy = model.PassportIssuedBy?.Trim(),
                PassportIssuedDate = model.PassportIssuedDate,
                PassportExpiryDate = model.PassportExpiryDate,
                Inn = model.Inn.Trim(),
                PassportAddress = model.PassportAddress.Trim(),
                ActualAddress = model.ActualAddress.Trim(),
                PassportFrontPath = frontPath,
                PassportBackPath = backPath,
                Status = UserStatus.Pending,
                RegisteredAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                await LoadRegisterViewData(serviceId);
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, "Client");

            var emailBody = $"<h2>{_localizer["RegisterSuccess"]}</h2>" +
                $"<p>{_localizer["Email"]}: {model.Email}</p>" +
                $"<p>{_localizer["Password"]}: <strong>{password}</strong></p>" +
                $"<p>{_localizer["RegisterEmailNote"]}</p>";

            await _emailService.SendAsync(model.Email, _localizer["RegisterTitle"], emailBody);

            TempData["Success"] = _localizer["RegisterSuccessPending"].Value;
            return RedirectToAction(nameof(Login));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadRegisterViewData(serviceId);
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToCabinet();

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, _localizer["LoginError"]);
            return View(model);
        }

        if (user.Status == UserStatus.Disabled)
        {
            ModelState.AddModelError(string.Empty, _localizer["AccountDisabled"]);
            return View(model);
        }

        if (user.Status == UserStatus.Pending)
        {
            ModelState.AddModelError(string.Empty, _localizer["AccountPending"]);
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, _localizer["LoginError"]);
            return View(model);
        }

        if (await _userManager.IsInRoleAsync(user, "Admin"))
            return RedirectToLocal(returnUrl) ?? RedirectToAction("Index", "Dashboard", new { area = "Admin" });

        return RedirectToLocal(returnUrl) ?? RedirectToAction("Index", "Dashboard", new { area = "Client" });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied() => View();

    private void ValidatePassportFiles(RegisterViewModel model)
    {
        if (model.PassportFront == null || model.PassportFront.Length == 0)
            ModelState.AddModelError(nameof(model.PassportFront), _localizer["Required"]);

        if (model.PassportBack == null || model.PassportBack.Length == 0)
            ModelState.AddModelError(nameof(model.PassportBack), _localizer["Required"]);
    }

    private async Task LoadRegisterViewData(int? serviceId)
    {
        ViewBag.ServiceId = serviceId;

        if (serviceId == null)
            return;

        var service = await _context.ServiceCategories.FindAsync(serviceId.Value);
        if (service == null)
            return;

        ViewBag.ServiceName = LocalizationHelper.IsTajik(HttpContext)
            ? service.TitleTj
            : service.TitleRu;
    }

    private IActionResult RedirectToCabinet()
    {
        if (User.IsInRole("Admin"))
            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

        return RedirectToAction("Index", "Dashboard", new { area = "Client" });
    }

    private IActionResult? RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return null;
    }
}
