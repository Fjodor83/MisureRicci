using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MisureRicci.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LockoutModel : PageModel
    {
        
        public void OnGet()
        {
            // This page is displayed when a user is locked out after too many failed login attempts.
        }
    }
}
