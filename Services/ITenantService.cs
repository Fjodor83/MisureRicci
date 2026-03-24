namespace MisureRicci.Services
{
    public interface ITenantService
    {
        int? GetCurrentNegozioId();
        bool IsAdmin();
        string? GetUserId();
    }
}
