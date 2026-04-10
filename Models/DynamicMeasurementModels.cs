using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace MisureRicci.Models
{
    public enum DynamicFieldType
    {
        Text = 0,
        Number = 1,
        Decimal = 2,
        Date = 3,
        Boolean = 4
    }

    public enum DynamicFieldTemplate
    {
        Standard = 0,
        Compact = 1,
        Emphasis = 2,
        Notes = 3,
        SectionHeader = 4,
        Divider = 5,
        AlertNote = 6,
        TwoColumnLocked = 7
    }

    public class MeasurementType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(80)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Descrizione { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsSystem { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [NotMapped]
        public IFormFile? ImageUpload { get; set; }

        public virtual ICollection<MeasurementFieldDefinition> Campi { get; set; } = new List<MeasurementFieldDefinition>();
    }

    public class MeasurementFieldDefinition
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MeasurementTypeId { get; set; }

        public virtual MeasurementType? MeasurementType { get; set; }

        [Required]
        [StringLength(80)]
        public string NomeCampo { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Etichetta { get; set; } = string.Empty;

        [StringLength(80)]
        public string? Gruppo { get; set; }

        public int OrdineGruppo { get; set; } = 0;

        public DynamicFieldType TipoDato { get; set; } = DynamicFieldType.Text;

        public DynamicFieldTemplate Template { get; set; } = DynamicFieldTemplate.Standard;

        [StringLength(20)]
        public string? UnitaMisura { get; set; }

        [StringLength(120)]
        public string? Placeholder { get; set; }

        [StringLength(160)]
        public string? HelpText { get; set; }

        public bool Obbligatorio { get; set; } = false;

        public int Ordine { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public virtual ICollection<DynamicMeasurementValue> Values { get; set; } = new List<DynamicMeasurementValue>();
    }

    public class DynamicMeasurementRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClienteId { get; set; }

        public virtual Cliente? Cliente { get; set; }

        [Required]
        public int MeasurementTypeId { get; set; }

        public virtual MeasurementType? MeasurementType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string? CreatedByUserId { get; set; }

        public virtual ApplicationUser? CreatedByUser { get; set; }

        public MeasurementUnit MeasurementUnit { get; set; } = MeasurementUnit.Centimeters;

        public virtual ICollection<DynamicMeasurementValue> Values { get; set; } = new List<DynamicMeasurementValue>();
    }

    public class DynamicMeasurementValue
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DynamicMeasurementRecordId { get; set; }

        public virtual DynamicMeasurementRecord? DynamicMeasurementRecord { get; set; }

        [Required]
        public int MeasurementFieldDefinitionId { get; set; }

        public virtual MeasurementFieldDefinition? MeasurementFieldDefinition { get; set; }

        [StringLength(1000)]
        public string? Valore { get; set; }
    }
}
