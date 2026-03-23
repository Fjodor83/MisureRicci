namespace MisureRicci.Models
{
    public static class ApplicationRoles
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Sartoria = "Sartoria";
        public const string Boutique = "Boutique";

        public static readonly string[] All =
        {
            Admin,
            Manager,
            Sartoria,
            Boutique
        };

        public static bool IsSupported(string? role)
        {
            return All.Contains(role, StringComparer.Ordinal);
        }
    }
}
