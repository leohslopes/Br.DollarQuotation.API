namespace Br.DollarQuotation.Domain.Interfaces.Services;

public interface IEmailService
{
    Task SendPasswordResetAsync(string recipientEmail, string recipientName, string resetLink, CancellationToken cancellationToken = default);
}