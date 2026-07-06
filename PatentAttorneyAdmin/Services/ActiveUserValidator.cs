using Microsoft.AspNetCore.Identity;
using PatentAttorneyAdmin.Models;

namespace PatentAttorneyAdmin.Services;

public class ActiveUserValidator : IUserValidator<ApplicationUser>
{
    public Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
    {
        if (user.Status == UserStatus.Disabled)
            return Task.FromResult(IdentityResult.Failed(new IdentityError
            {
                Code = "UserDisabled",
                Description = "User account is disabled."
            }));

        return Task.FromResult(IdentityResult.Success);
    }
}
