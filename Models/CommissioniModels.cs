using System.ComponentModel.DataAnnotations;

namespace MisureRicci.Models
{
    public enum StatoCommessa
    {
        Bozza = 0,
        MisureRaccolte = 1,
        InLavorazione = 2,
        Prova1 = 3,
        Prova2 = 4,
        ProntaConsegna = 5,
        Consegnata = 6,
        Annullata = 7
    }

    public class CommessaSartoriale
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Codice Commessa")]
        [StringLength(30)]
        public string? CommessaCode { get; set; }

        [Required]
        public int ClienteId { get; set; }
        public virtual Cliente? Cliente { get; set; }

        public int? NegozioId { get; set; }
        public virtual Negozio? Negozio { get; set; }

        [Required]
        [Display(Name = "Tipo Capo")]
        [StringLength(80)]
        public string TipoCapo { get; set; } = string.Empty;

        [Display(Name = "Tessuto")]
        [StringLength(120)]
        public string? Tessuto { get; set; }

        [Display(Name = "Collezione")]
        [StringLength(120)]
        public string? Collezione { get; set; }

        [Display(Name = "Data Apertura")]
        public DateTime DataApertura { get; set; } = DateTime.UtcNow;

        [Display(Name = "Consegna Prevista")]
        public DateTime? DataConsegnaPrevista { get; set; }

        [Display(Name = "Consegna Effettiva")]
        public DateTime? DataConsegnaEffettiva { get; set; }

        [Display(Name = "Stato")]
        public StatoCommessa Stato { get; set; } = StatoCommessa.Bozza;

        [Display(Name = "Note Interne")]
        [StringLength(2000)]
        public string? NoteInterne { get; set; }

        [StringLength(450)]
        public string? CreatedByUserId { get; set; }
        public virtual ApplicationUser? CreatedByUser { get; set; }

        public virtual ICollection<CommessaEvento> Eventi { get; set; } = new List<CommessaEvento>();
        public virtual ICollection<CommessaMisuraLink> MisureCollegate { get; set; } = new List<CommessaMisuraLink>();
    }

    public class CommessaEvento
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CommessaSartorialeId { get; set; }
        public virtual CommessaSartoriale? CommessaSartoriale { get; set; }

        [Required]
        [StringLength(40)]
        public string TipoEvento { get; set; } = "Nota";

        public StatoCommessa? NuovoStato { get; set; }

        [Required]
        [StringLength(1000)]
        public string Descrizione { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string? CreatedByUserId { get; set; }
        public virtual ApplicationUser? CreatedByUser { get; set; }
    }

    public class CommessaMisuraLink
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CommessaSartorialeId { get; set; }
        public virtual CommessaSartoriale? CommessaSartoriale { get; set; }

        [Required]
        public int MisuraClienteId { get; set; }
        public virtual MisureCliente? MisuraCliente { get; set; }

        public DateTime LinkedAt { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string? LinkedByUserId { get; set; }
        public virtual ApplicationUser? LinkedByUser { get; set; }
    }

    public class CommessaMisuraItem
    {
        public int MisuraClienteId { get; set; }
        public int RecordId { get; set; }
        public string TipoMisura { get; set; } = string.Empty;
        public bool IsDynamic { get; set; }
        public DateTime DataCreazione { get; set; }
        public string? Note { get; set; }
    }
}
