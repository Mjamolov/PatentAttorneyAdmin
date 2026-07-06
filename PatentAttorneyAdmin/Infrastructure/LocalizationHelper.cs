namespace PatentAttorneyAdmin.Infrastructure;

public static class LocalizationHelper
{
    public static string CurrentCulture(HttpContext context) =>
        context.Features.Get<Microsoft.AspNetCore.Localization.IRequestCultureFeature>()?.RequestCulture.Culture.Name ?? "ru";

    public static bool IsTajik(HttpContext context) =>
        CurrentCulture(context).StartsWith("tg", StringComparison.OrdinalIgnoreCase);
}
