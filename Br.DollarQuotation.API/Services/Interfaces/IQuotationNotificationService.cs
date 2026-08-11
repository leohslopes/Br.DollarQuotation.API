using Br.DollarQuotation.Domain.Entities;
using Br.DollarQuotation.Messaging.Contracts;

namespace Br.DollarQuotation.API.Services.Interfaces;

public interface IQuotationNotificationService
{
    Task NotifyQuotationUpdatedAsync(QuotationUpdatedMessage quotation, CancellationToken cancellationToken = default);

    Task NotifyQuotationAlertTriggeredAsync(QuotationAlert alert, decimal currentPrice, CancellationToken cancellationToken = default);
}