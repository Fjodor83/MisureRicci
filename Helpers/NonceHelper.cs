using Microsoft.AspNetCore.Http;

namespace MisureRicci.Helpers
{
    public static class NonceHelper
    {
        public static string GetNonce(this IHttpContextAccessor httpContextAccessor)
        {
            return httpContextAccessor.HttpContext?.Items["CSP-Nonce"]?.ToString() ?? string.Empty;
        }

        public static string GetNonce(HttpContext context)
        {
            return context?.Items["CSP-Nonce"]?.ToString() ?? string.Empty;
        }
    }
}
