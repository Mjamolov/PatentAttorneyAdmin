using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace PatentAttorneyAdmin.Controllers;

public class CultureController : Controller
{
    private static readonly HashSet<string> SupportedCultures = new(StringComparer.OrdinalIgnoreCase) { "ru", "tg" };

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public IActionResult Set(string culture, string returnUrl)
    {
        if (!SupportedCultures.Contains(culture))
            culture = "ru";

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                Path = "/",
                SameSite = SameSiteMode.Lax,
                HttpOnly = false
            });

        if (string.IsNullOrWhiteSpace(returnUrl) || !Url.IsLocalUrl(returnUrl))
            returnUrl = "/";

        return LocalRedirect(returnUrl);
    }
}
