using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using MisureRicci.Models;

namespace MisureRicci.Services
{
    public class TenantService : ITenantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? GetCurrentNegozioId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !user.Identity?.IsAuthenticated == true)
            {
                return null;
            }

            var claim = user.FindFirst("NegozioId");
            if (claim != null && int.TryParse(claim.Value, out int negozioId))
            {
                return negozioId;
            }

            return null;
        }

        public bool IsAdmin()
        {
            return _httpContextAccessor.HttpContext?.User?.IsInRole(ApplicationRoles.Admin) ?? false;
        }

        public string? GetUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
