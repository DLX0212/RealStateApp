using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using RealEstateApp.Core.Application.Interfaces.Infrastructure;
using RealEstateApp.Core.Application.Settings;


namespace RealEstateApp.Infrastructure.Identity.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendActivationEmailAsync(string toEmail, string firstName, string activationLink)
        {
            var subject = "Activate your RealEstateApp account";
            var body = $@"
            <div style='font-family:Segoe UI,Arial,sans-serif;max-width:520px;margin:0 auto;'>
                <h2 style='color:#1e3a5f;'>Welcome, {firstName}!</h2>
                <p>Thank you for registering with RealEstateApp. Please confirm your account by clicking the button below.</p>
                <p style='text-align:center;margin:24px 0;'>
                    <a href='{activationLink}' style='background:#1e3a5f;color:#fff;padding:12px 24px;
                       text-decoration:none;border-radius:6px;display:inline-block;'>Activate Account</a>
                </p>
                <p>If the button does not work, copy and paste this link into your browser:</p>
                <p><a href='{activationLink}'>{activationLink}</a></p>
            </div>";

            using var message = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                Credentials = new NetworkCredential(_settings.SenderEmail, _settings.SenderPassword),
                EnableSsl = _settings.EnableSsl
            };

            await client.SendMailAsync(message);
        }
    }
}
