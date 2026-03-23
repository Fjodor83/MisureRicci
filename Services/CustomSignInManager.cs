using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MisureRicci.Models;

namespace MisureRicci.Services;

public class CustomSignInManager : SignInManager<ApplicationUser>
{
    public CustomSignInManager(
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor contextAccessor,
        IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
        IOptions<IdentityOptions> optionsAccessor,
        ILogger<SignInManager<ApplicationUser>> logger,
        IAuthenticationSchemeProvider schemes,
        IUserConfirmation<ApplicationUser> confirmation)
        : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    {
    }

    public override async Task<SignInResult> CheckPasswordSignInAsync(
        ApplicationUser user,
        string password,
        bool lockoutOnFailure)
    {
        if (!user.Attivo)
        {
            Logger.LogWarning("Login negato per utente disabilitato: {UserId}", user.Id);
            return SignInResult.NotAllowed;
        }

        return await base.CheckPasswordSignInAsync(user, password, lockoutOnFailure);
    }

    public override async Task<bool> CanSignInAsync(ApplicationUser user)
    {
        if (!user.Attivo)
        {
            return false;
        }

        return await base.CanSignInAsync(user);
    }
}
