namespace MisureRicci.Models.Options
{
    public class StorageOptions
    {
        public const string SectionName = "Storage";

        public string Provider { get; set; } = "Local";

        // If empty, storage falls back to <ContentRoot>/SecureUploads.
        public string? LocalBasePath { get; set; }

        public string? AzureBlobConnectionString { get; set; }

        public string? ContainerName { get; set; }
    }
}
