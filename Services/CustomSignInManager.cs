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
            Logger.LogWarning("Login negato per utente disabilitato: {UserEmail} (ID: {UserId})", user.Email, user.Id);
            return SignInResult.NotAllowed;
        }

        var result = await base.CheckPasswordSignInAsync(user, password, lockoutOnFailure);

        if (result.Succeeded)
        {
            Logger.LogInformation("Login effettuato con successo: {UserEmail} (ID: {UserId})", user.Email, user.Id);
        }
        else if (result.IsLockedOut)
        {
            Logger.LogCritical("Account BLOCCATO per troppi tentativi falliti: {UserEmail} (ID: {UserId})", user.Email, user.Id);
        }
        else if (result.IsNotAllowed)
        {
            Logger.LogWarning("Accesso non consentito per l'utente: {UserEmail} (ID: {UserId})", user.Email, user.Id);
        }
        else if (!result.Succeeded)
        {
            Logger.LogWarning("Tentativo di login FALLITO per l'utente: {UserEmail} (ID: {UserId})", user.Email, user.Id);
        }

        return result;
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
