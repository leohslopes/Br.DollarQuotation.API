using Br.DollarQuotation.Application.DTOs.Responses;

namespace Br.DollarQuotation.Worker.Services.Interfaces;

public interface IQuotationNotificationClient
{
    Task NotifyAsync(CurrencyQuotationResponse quotation, CancellationToken cancellationToken = default);
}