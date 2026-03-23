using System.ComponentModel.DataAnnotations;

namespace MisureRicci.Models.ViewModels
{
    public class CommessaKpiViewModel
    {
        public int Totale { get; set; }
        public int InCorso { get; set; }
        public int Consegnate { get; set; }
        public int InRitardo { get; set; }
    }

    /// <summary>
    /// Snapshot dello stato misure per la commessa: usato dalla view per scegliere
    /// quale pannello contestuale mostrare (collega esistente vs crea nuova).
    /// </summary>
    public class CommessaMisuraStatus
    {
        /// <summary>True se alla commessa è collegata almeno una misura.</summary>
        public bool HasMisureCollegate { get; set; }

        /// <summary>True se il cliente ha misure nel RegistroMisure non ancora collegate a questa commessa.</summary>
        public bool HasMisureDisponibili { get; set; }

        /// <summary>
        /// True se il cliente non ha ancora nessuna misura nel RegistroMisure.
        /// In questo caso la view deve guidare l'operatore alla creazione di una nuova misura dinamica.
        /// </summary>
        public bool RequireMisuraCreation { get; set; }

        /// <summary>Numero totale di misure nel RegistroMisure per il cliente.</summary>
        public int TotaleMisureCliente { get; set; }
    }

    public class CommessaDetailsViewModel
    {
        public CommessaSartoriale Commessa { get; set; } = new();
        public List<StatoCommessa> StatiDisponibili { get; set; } = new();
        public List<CommessaMisuraItem> MisureDisponibili { get; set; } = new();
        public List<CommessaMisuraItem> MisureCollegate { get; set; } = new();

        /// <summary>Snapshot dello stato misure per questa commessa.</summary>
        public CommessaMisuraStatus MisuraStatus { get; set; } = new();

        /// <summary>
        /// Tipi di misura dinamica attivi, popolati quando RequireMisuraCreation == true
        /// o comunque per permettere la creazione di ulteriori misure dalla stessa pagina.
        /// </summary>
        public List<MeasurementType> MeasurementTypes { get; set; } = new();
    }

    public class CommessaCreateViewModel
    {
        [Required]
        public int ClienteId { get; set; }

        public string ClienteNome { get; set; } = string.Empty;

        [Required]
        [StringLength(80)]
        [Display(Name = "Tipo Capo")]
        public string TipoCapo { get; set; } = string.Empty;

        [StringLength(120)]
        [Display(Name = "Tessuto")]
        public string? Tessuto { get; set; }

        [StringLength(120)]
        [Display(Name = "Collezione")]
        public string? Collezione { get; set; }

        [Display(Name = "Consegna Prevista")]
        [DataType(DataType.Date)]
        public DateTime? DataConsegnaPrevista { get; set; }

        [StringLength(2000)]
        [Display(Name = "Note Interne")]
        public string? NoteInterne { get; set; }

        /// <summary>Lista per visualizzare le misure esistenti del cliente in fase di creazione.</summary>
        public List<CommessaMisuraItem> MisureDisponibili { get; set; } = new();

        /// <summary>ID delle misure selezionate dall'utente durante la creazione.</summary>
        public List<int> SelectedMisuraIds { get; set; } = new();
    }
}
