using System;
using System.ComponentModel.DataAnnotations;

namespace MisureRicci.Models
{
    public abstract class BaseMeasurement
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ClienteId { get; set; }
        
        public virtual Cliente? Cliente { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public string? OrderId { get; set; }
        
        public string? Notes { get; set; }
    }

    public class GiaccaMeasurement : BaseMeasurement
    {
        [Display(Name = "Spalle (Shoulders)")]
        public double Spalle { get; set; }

        [Display(Name = "Torace (Chest)")]
        public double Torace { get; set; }

        [Display(Name = "Vita (Waist)")]
        public double Vita { get; set; }

        [Display(Name = "Lunghezza Manica (Sleeve Length)")]
        public double Manica { get; set; }

        [Display(Name = "Lunghezza Totale (Total Length)")]
        public double Lunghezza { get; set; }
    }

    public class PantaloneMeasurement : BaseMeasurement
    {
        [Display(Name = "Vita (Waist)")]
        public double Vita { get; set; }

        [Display(Name = "Bacino (Hips)")]
        public double Bacino { get; set; }

        [Display(Name = "Cavallo (Rise)")]
        public double Cavallo { get; set; }

        [Display(Name = "Interno Gamba (Inseam)")]
        public double InternoGamba { get; set; }

        [Display(Name = "Fondo (Bottom)")]
        public double Fondo { get; set; }
    }

    public class AbitoCompletoMeasurement : BaseMeasurement
    {
        // Composizione di Giacca e Pantalone
        public GiaccaMeasurement Giacca { get; set; } = new();
        public PantaloneMeasurement Pantalone { get; set; } = new();
    }

    public class GiletMeasurement : BaseMeasurement
    {
        [Display(Name = "Torace (Chest)")]
        public double Torace { get; set; }

        [Display(Name = "Vita (Waist)")]
        public double Vita { get; set; }

        [Display(Name = "Lunghezza (Length)")]
        public double Lunghezza { get; set; }
    }

    public class MaglieMeasurement : BaseMeasurement
    {
        [Display(Name = "Torace (Chest)")]
        public double Torace { get; set; }

        [Display(Name = "Spalle (Shoulders)")]
        public double Spalle { get; set; }

        [Display(Name = "Manica (Sleeve)")]
        public double Manica { get; set; }

        [Display(Name = "Lunghezza (Length)")]
        public double Lunghezza { get; set; }
    }

    public class OutdoorMeasurement : BaseMeasurement
    {
        [Display(Name = "Torace (Chest)")]
        public double Torace { get; set; }

        [Display(Name = "Spalle (Shoulders)")]
        public double Spalle { get; set; }

        [Display(Name = "Manica (Sleeve)")]
        public double Manica { get; set; }

        [Display(Name = "Lunghezza (Length)")]
        public double Lunghezza { get; set; }

        [Display(Name = "Vestibilità (Fit)")]
        public string? Fit { get; set; }
    }

    public class CamiciaMeasurement : BaseMeasurement
    {
        [Display(Name = "Collo (Neck)")]
        public double Collo { get; set; }

        [Display(Name = "Spalle (Shoulders)")]
        public double Spalle { get; set; }

        [Display(Name = "Torace (Chest)")]
        public double Torace { get; set; }

        [Display(Name = "Vita (Waist)")]
        public double Vita { get; set; }

        [Display(Name = "Manica (Sleeve)")]
        public double Manica { get; set; }

        [Display(Name = "Polso (Wrist)")]
        public double Polso { get; set; }

        [Display(Name = "Lunghezza (Length)")]
        public double Lunghezza { get; set; }
    }

    public class ScarpeMeasurement : BaseMeasurement
    {
        [Display(Name = "Taglia (Size - EU/US/UK)")]
        public string? Taglia { get; set; }

        [Display(Name = "Lunghezza Piede (Foot Length - cm)")]
        public double LunghezzaPiede { get; set; }

        [Display(Name = "Larghezza Pianta (Plant Width)")]
        public string? Pianta { get; set; }
    }

    public class CravattaMeasurement : BaseMeasurement
    {
        [Display(Name = "Lunghezza (Length)")]
        public double Lunghezza { get; set; }

        [Display(Name = "Larghezza (Width)")]
        public double Larghezza { get; set; }
    }

    public class CinturaMeasurement : BaseMeasurement
    {
        [Display(Name = "Lunghezza Totale (Total Length)")]
        public double Lunghezza { get; set; }

        [Display(Name = "Girovita (Waist Line)")]
        public double Girovita { get; set; }
    }
}

