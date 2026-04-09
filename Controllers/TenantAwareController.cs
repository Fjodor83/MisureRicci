using Microsoft.AspNetCore.Mvc;
using MisureRicci.Services;

namespace MisureRicci.Controllers
{
    public abstract class TenantAwareController : Controller
    {
        protected readonly ITenantService TenantService;

        protected TenantAwareController(ITenantService tenantService)
        {
            TenantService = tenantService;
        }

        protected bool IsAdmin => TenantService.IsAdmin();
        protected int? NegozioId => TenantService.GetCurrentNegozioId();
        protected string? UserId => TenantService.GetUserId();

        // Backward-compatible aliases used by older controllers.
        protected int? CurrentNegozioId => NegozioId;
        protected string? CurrentUserId => UserId;

        protected IActionResult? RequireTenant()
        {
            if (!IsAdmin && !NegozioId.HasValue)
                return View("TenantAssignmentRequired");
            return null;
        }
    }
}
