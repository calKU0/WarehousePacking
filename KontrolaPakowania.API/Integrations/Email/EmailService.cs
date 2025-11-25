using Azure.Messaging;
using KontrolaPakowania.API.Settings;
using KontrolaPakowania.Shared.DTOs;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace KontrolaPakowania.API.Integrations.Email
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _settings;
        private readonly SmtpClient _smtp;

        public EmailService(IOptions<SmtpSettings> options)
        {
            _settings = options.Value;
            _smtp = new SmtpClient(_settings.Host, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.User, _settings.Password),
                EnableSsl = _settings.EnableSsl
            };
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            using var mail = new MailMessage()
            {
                From = new MailAddress(_settings.From),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            // Split recipients by semicolon or comma and add them
            var recipients = to.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var recipient in recipients)
            {
                mail.To.Add(recipient.Trim());
            }

            await _smtp.SendMailAsync(mail);
        }

        public async Task SendPackageFailureEmail(PackageData package, string errorMessage)
        {
            var subject = $"❌ Błąd wysyłki paczki dla klienta {package.Recipient.Name}";
            var body = $@"
            <div style='font-family: Arial, sans-serif; font-size: 14px; color: #333; line-height: 1.5;'>
                <p style='font-size:16px; font-weight:bold; color:#d9534f;'>
                    Wystąpił błąd przy próbie wysłania paczki do kontrahenta: {package.Recipient.Name}
                </p>

                <p>
                    <strong>Dokument handlowy:</strong> {package.InvoiceName} <br>
                    <strong>Paczka:</strong> {package.PackageName}
                </p>

                <p>
                    Błąd jest najprawdopodobniej spowodowany błędnym adresem docelowym.
                    Popraw adres docelowy na karcie kontrahenta!
                </p>

                <p>Zwróć szczególną uwagę na pola:</p>
                <ul style='list-style-type: disc; margin-left:20px;'>
                    <li>Kod pocztowy</li>
                    <li>Miasto</li>
                    <li>Telefon oraz Email</li>
                </ul>";

            if (!string.IsNullOrEmpty(errorMessage))
            {
                body += $@"
                <p style='margin-top:20px;'>
                    <strong>Szczegóły błędu:</strong><br>
                    {errorMessage}
                </p>";
            }

            body += @"
                <hr style='border:none; border-top:1px solid #ccc; margin:20px 0;'>

                <p>Pozdrawiamy,<br>
                <strong>Magazyn</strong></p>

                <p style='font-size:12px; color:#777;'>
                    <i>Wiadomość wygenerowana automatycznie przez program do wysyłki paczek.</i>
                </p>
            </div>";

            if (!string.IsNullOrWhiteSpace(package.RepresentativeEmail))
            {
                await SendEmailAsync(package.RepresentativeEmail, subject, body);
            }
        }
    }
}