namespace Br.DollarQuotation.Domain.Interfaces.Services;

public interface IEmailService
{
    Task SendPasswordResetAsync(string recipientEmail, string recipientName, string resetLink, CancellationToken cancellationToken = default);

    Task SendQuotationAlertTriggeredAsync(string recipientEmail, string recipientName, string currencyPair, decimal currentPrice, decimal targetPrice,  string condition, CancellationToken cancellationToken = default);
}