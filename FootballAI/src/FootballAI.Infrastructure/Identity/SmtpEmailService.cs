using FootballAI.Application.Interfaces.AuthInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace FootballAI.Infrastructure.Identity;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendEmailConfirmationAsync(
        string email, string confirmationToken, CancellationToken ct = default)
    {
        var baseUrl = _config["App:BaseUrl"];
        var link = $"{baseUrl}/api/auth/confirm-email?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(confirmationToken)}";

        var body = $@"
            <h2>Welcome to FootballAI!</h2>
            <p>Please confirm your email address by clicking the link below:</p>
            <p><a href='{link}'>Confirm my email</a></p>
            <p>If you did not register, please ignore this message.</p>";

        await SendEmailAsync(email, "Confirm your FootballAI account", body, ct);
    }

    public async Task SendPasswordResetAsync(
        string email, string resetToken, CancellationToken ct = default)
    {
        var baseUrl = _config["App:BaseUrl"];
        var link = $"{baseUrl}/reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(resetToken)}";

        var body = $@"
            <h2>Password Reset Request</h2>
            <p>Click the link below to reset your password:</p>
            <p><a href='{link}'>Reset my password</a></p>
            <p>This link expires in 24 hours. If you did not request this, please ignore this email.</p>";

        await SendEmailAsync(email, "FootballAI password reset", body, ct);
    }

    private async Task SendEmailAsync(string to, string subject, string body, CancellationToken ct)
    {
        try
        {
            using var client = new SmtpClient(_config["Email:SmtpHost"], int.Parse(_config["Email:SmtpPort"]!))
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(
                    _config["Email:Username"], _config["Email:Password"])
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_config["Email:From"]!, "FootballAI"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(to);

            await client.SendMailAsync(message, ct);
            _logger.LogInformation("Email sent to {To}: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            throw;
        }
    }
}
