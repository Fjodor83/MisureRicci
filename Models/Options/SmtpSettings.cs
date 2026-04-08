namespace MisureRicci.Models.Options
{
    /// <summary>
    /// Impostazioni SMTP per l'invio di email transazionali.
    /// In produzione, configurare tramite variabili d'ambiente con prefisso Smtp__
    /// (es. Smtp__Host, Smtp__Port, Smtp__User, Smtp__Password, Smtp__From).
    /// </summary>
    public class SmtpSettings
    {
        public const string SectionName = "Smtp";

        public string? Host { get; set; }
        public int Port { get; set; } = 587;
        public string? User { get; set; }
        public string? Password { get; set; }
        public string? From { get; set; }
    }
}
