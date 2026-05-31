using MonitoringPlatform.Application.Interfaces;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Logging;

namespace MonitoringPlatform.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    // In a real application, configuration would be injected (e.g., SmtpSettings)
    private readonly string _smtpHost = "smtp.mailtrap.io"; // Example with Mailtrap
    private readonly int _smtpPort = 2525;
    private readonly string _smtpUser = "your_mailtrap_username";
    private readonly string _smtpPass = "your_mailtrap_password";
    private readonly string _fromEmail = "no-reply@monitoringplatform.com";

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            using var client = new SmtpClient(_smtpHost, _smtpPort)
            {
                Credentials = new NetworkCredential(_smtpUser, _smtpPass),
                EnableSsl = true // Use SSL/TLS for secure communication
            };

            var mailMessage = new MailMessage(
                from: _fromEmail,
                to: to,
                subject: subject,
                body: body
            );
            mailMessage.IsBodyHtml = true;

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully to {To} with subject {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To} with subject {Subject}", to, subject);
            throw; // Re-throw to be handled by notification retry logic
        }
    }
}
