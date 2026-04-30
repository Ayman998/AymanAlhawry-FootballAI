namespace FootballAI.Application.Interfaces.AuthInterfaces;

public interface IEmailService
{
    Task SendEmailConfirmationAsync(string email, string confirmationToken, CancellationToken ct = default);
    Task SendPasswordResetAsync(string email, string resetToken, CancellationToken ct = default);
}
