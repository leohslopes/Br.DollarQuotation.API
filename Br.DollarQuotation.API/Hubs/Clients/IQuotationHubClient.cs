using Br.DollarQuotation.Application.DTOs.Responses;

namespace Br.DollarQuotation.API.Hubs.Clients;

public interface IQuotationHubClient
{
    Task QuotationUpdated(CurrencyQuotationResponse quotation);

    Task QuotationAlertTriggered(QuotationAlertTriggeredResponse alert);
}