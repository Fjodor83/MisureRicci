using System.ComponentModel.DataAnnotations;

namespace MisureRicci.Models.ViewModels
{
    public class DynamicFieldInputViewModel
    {
        public int FieldDefinitionId { get; set; }
        public string? NomeCampo { get; set; }
        public string? Etichetta { get; set; }
        public string? Gruppo { get; set; }
        public int OrdineGruppo { get; set; }
        public DynamicFieldType TipoDato { get; set; }
        public DynamicFieldTemplate Template { get; set; }
        public string? UnitaMisura { get; set; }
        public string? Placeholder { get; set; }
        public string? HelpText { get; set; }
        public bool Obbligatorio { get; set; }
        public int Ordine { get; set; }
        public string? Valore { get; set; }
    }

    public class DynamicMeasurementCreateViewModel
    {
        public int RecordId { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [Required]
        public int MeasurementTypeId { get; set; }

        [Required]
        public MeasurementUnit SelectedUnit { get; set; } = MeasurementUnit.Centimeters;

        public string ClienteNome { get; set; } = string.Empty;
        public string TipoNome { get; set; } = string.Empty;

        /// <summary>
        /// Se valorizzato, al salvataggio la misura viene collegata automaticamente
        /// alla commessa indicata e il redirect torna a Commissioni/Details.
        /// </summary>
        public int? ReturnToCommessaId { get; set; }

        public string? TypeImageUrl { get; set; }

        public List<DynamicFieldInputViewModel> Fields { get; set; } = new();
    }

    public class MeasurementTypeManageViewModel
    {
        public MeasurementType Type { get; set; } = new();
        public List<MeasurementFieldDefinition> Fields { get; set; } = new();
    }
}
