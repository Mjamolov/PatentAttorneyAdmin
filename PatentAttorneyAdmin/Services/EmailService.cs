using System.Net;
using System.Net.Mail;

namespace PatentAttorneyAdmin.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        if (string.IsNullOrWhiteSpace(to))
            return;

        var host = _configuration["Smtp:Host"];
        var port = _configuration.GetValue<int?>("Smtp:Port") ?? 587;
        var user = _configuration["Smtp:User"];
        var password = _configuration["Smtp:Password"];
        var from = _configuration["Smtp:From"] ?? user;
        var displayName = _configuration["Smtp:DisplayName"];
        var enableSsl = _configuration.GetValue<bool?>("Smtp:EnableSsl") ?? true;
        var timeout = _configuration.GetValue<int?>("Smtp:Timeout") ?? 60000;
        var isBodyHtml = _configuration.GetValue<bool?>("Smtp:IsBodyHtml") ?? true;

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
        {
            _logger.LogWarning("SMTP not configured. Email to {To} with subject '{Subject}' was not sent.", to, subject);
            return;
        }

        using var message = new MailMessage
        {
            From = string.IsNullOrWhiteSpace(displayName)
                ? new MailAddress(from)
                : new MailAddress(from, displayName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = isBodyHtml
        };
        message.To.Add(to);

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl,
            Timeout = timeout,
            Credentials = string.IsNullOrWhiteSpace(user)
                ? CredentialCache.DefaultNetworkCredentials
                : new NetworkCredential(user, password)
        };

        await client.SendMailAsync(message);
        _logger.LogInformation("Email sent to {To}", to);
    }
}
