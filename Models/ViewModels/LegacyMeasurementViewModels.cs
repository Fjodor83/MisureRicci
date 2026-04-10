using System.ComponentModel.DataAnnotations;

namespace MisureRicci.Models.ViewModels
{
    public class LegacyMeasurementFieldViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Value { get; set; }
        public bool IsMultiline { get; set; }
    }

    public class LegacyMeasurementSectionViewModel
    {
        public string Title { get; set; } = string.Empty;
        public List<LegacyMeasurementFieldViewModel> Fields { get; set; } = new();
    }

    public class LegacyMeasurementEditViewModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public int ClienteId { get; set; }
        public string TipoMisura { get; set; } = string.Empty;
        public List<LegacyMeasurementFieldViewModel> Fields { get; set; } = new();
        [Required]
        public bool CanEditFields { get; set; }
        public string? WarningMessage { get; set; }
    }

    public class LegacyMeasurementDetailsViewModel
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string TipoMisura { get; set; } = string.Empty;
        public string ClienteNome { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? Notes { get; set; }
        public List<LegacyMeasurementFieldViewModel> Fields { get; set; } = new();
        public List<LegacyMeasurementSectionViewModel> Sections { get; set; } = new();
    }

    public class LegacyMeasurementDeleteViewModel
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string TipoMisura { get; set; } = string.Empty;
        public int? RegistryId { get; set; }
    }
}