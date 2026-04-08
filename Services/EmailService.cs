using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MisureRicci.Models.Options;

namespace MisureRicci.Services
{
    /// <summary>
    /// Implementazione di <see cref="IEmailSender"/> per ASP.NET Identity.
    /// In Development logga l'email; in Production invia tramite SMTP.
    /// Non propaga mai eccezioni verso il chiamante.
    /// </summary>
    public class EmailService : IEmailSender
    {
        private readonly SmtpSettings _smtp;
        private readonly ILogger<EmailService> _logger;
        private readonly IWebHostEnvironment _env;

        public EmailService(
            IOptions<SmtpSettings> smtpOptions,
            ILogger<EmailService> logger,
            IWebHostEnvironment env)
        {
            _smtp = smtpOptions.Value;
            _logger = logger;
            _env = env;
        }

        /// <summary>
        /// Invia un'email. In Development scrive nel log; in Production usa SMTP.
        /// </summary>
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                if (_env.IsDevelopment())
                {
                    _logger.LogWarning(
                        "[DEV EMAIL] To: {To} | Subject: {Subject} | Body: {Body}",
                        email, subject, htmlMessage);
                    return;
                }

                if (string.IsNullOrWhiteSpace(_smtp.Host) || string.IsNullOrWhiteSpace(_smtp.From))
                {
                    _logger.LogWarning(
                        "SMTP non configurato. Email non inviata a {To} con oggetto '{Subject}'.",
                        email, subject);
                    return;
                }

                using var message = new MailMessage();
                message.From = new MailAddress(_smtp.From);
                message.To.Add(new MailAddress(email));
                message.Subject = subject;
                message.Body = htmlMessage;
                message.IsBodyHtml = true;

                using var client = new SmtpClient(_smtp.Host, _smtp.Port);
                client.EnableSsl = true;

                if (!string.IsNullOrWhiteSpace(_smtp.User) && !string.IsNullOrWhiteSpace(_smtp.Password))
                {
                    client.Credentials = new NetworkCredential(_smtp.User, _smtp.Password);
                }

                await client.SendMailAsync(message);
                _logger.LogInformation("Email inviata a {To} con oggetto '{Subject}'.", email, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'invio dell'email a {To} con oggetto '{Subject}'.", email, subject);
            }
        }
    }
}
