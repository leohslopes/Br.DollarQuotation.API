using Br.DollarQuotation.Application.DTOs.Responses;

namespace Br.DollarQuotation.API.Services.Interfaces;

public interface IQuotationNotificationService
{
    Task NotifyQuotationUpdatedAsync(CurrencyQuotationResponse quotation, CancellationToken cancellationToken = default);
}