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

    public class CommessaDetailsViewModel
    {
        public CommessaSartoriale Commessa { get; set; } = new();
        public List<StatoCommessa> StatiDisponibili { get; set; } = new();
        public List<CommessaMisuraItem> MisureDisponibili { get; set; } = new();
        public List<CommessaMisuraItem> MisureCollegate { get; set; } = new();
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
    }
}
