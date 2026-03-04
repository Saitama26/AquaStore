namespace Common.Application.Abstractions.Services;

/// <summary>
/// Сервис отправки email
/// </summary>
public interface IEmailSender
{
    Task SendEmailAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default);
}

