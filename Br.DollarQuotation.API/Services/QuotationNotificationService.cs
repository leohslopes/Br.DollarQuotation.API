using Br.DollarQuotation.API.Hubs;
using Br.DollarQuotation.API.Hubs.Clients;
using Br.DollarQuotation.API.Services.Interfaces;
using Br.DollarQuotation.Application.DTOs.Responses;
using Microsoft.AspNetCore.SignalR;

namespace Br.DollarQuotation.API.Services;

public sealed class QuotationNotificationService: IQuotationNotificationService
{
    private readonly IHubContext<QuotationHub, IQuotationHubClient> _hubContext;

    private readonly ILogger<QuotationNotificationService> _logger;

    public QuotationNotificationService(
        IHubContext<QuotationHub, IQuotationHubClient> hubContext,
        ILogger<QuotationNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyQuotationUpdatedAsync(CurrencyQuotationResponse quotation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(quotation);

        await _hubContext.Clients.All.QuotationUpdated(quotation);

        _logger.LogInformation("Cotação {CurrencyPair} enviada aos clientes conectados.", quotation.CurrencyPair);
    }
}
