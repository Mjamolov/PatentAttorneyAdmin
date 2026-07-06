using Microsoft.Extensions.Localization;
using PatentAttorneyAdmin.Models;
using PatentAttorneyAdmin.Resources;

namespace PatentAttorneyAdmin.Infrastructure;

public static class LocalizationHelper
{
    public static string CurrentCulture(HttpContext context) =>
        context.Features.Get<Microsoft.AspNetCore.Localization.IRequestCultureFeature>()?.RequestCulture.Culture.Name ?? "ru";

    public static bool IsTajik(HttpContext context) =>
        CurrentCulture(context).StartsWith("tg", StringComparison.OrdinalIgnoreCase);

    public static string Get(IStringLocalizer<SharedResource> localizer, string key) =>
        localizer[key].Value;

    public static string ApplicationStatus(IStringLocalizer<SharedResource> localizer, ServiceApplicationStatus status) =>
        status switch
        {
            ServiceApplicationStatus.Submitted => localizer["AppStatusSubmitted"].Value,
            ServiceApplicationStatus.UnderReview => localizer["AppStatusUnderReview"].Value,
            ServiceApplicationStatus.Approved => localizer["AppStatusApproved"].Value,
            ServiceApplicationStatus.Rejected => localizer["AppStatusRejected"].Value,
            ServiceApplicationStatus.NeedsCorrection => localizer["AppStatusNeedsCorrection"].Value,
            _ => status.ToString()
        };

    public static string UserStatusLabel(IStringLocalizer<SharedResource> localizer, UserStatus status) =>
        status switch
        {
            UserStatus.Pending => localizer["StatusPending"].Value,
            UserStatus.Active => localizer["StatusActive"].Value,
            _ => localizer["StatusDisabled"].Value
        };

    public static string RequestTypeLabel(IStringLocalizer<SharedResource> localizer, RequestType type) =>
        type switch
        {
            RequestType.Invitation => localizer["TypeInvitation"].Value,
            RequestType.DocumentCorrection => localizer["TypeDocumentCorrection"].Value,
            _ => localizer["TypeGeneral"].Value
        };

    public static string RequestStatusLabel(IStringLocalizer<SharedResource> localizer, RequestStatus status) =>
        status switch
        {
            RequestStatus.Open => localizer["RequestStatusOpen"].Value,
            RequestStatus.InProgress => localizer["RequestStatusInProgress"].Value,
            RequestStatus.AwaitingClient => localizer["RequestStatusAwaitingClient"].Value,
            RequestStatus.Resolved => localizer["RequestStatusResolved"].Value,
            _ => localizer["RequestStatusClosed"].Value
        };
}
