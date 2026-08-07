using Br.DollarQuotation.Application.DTOs.Responses;
using Br.DollarQuotation.Worker.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Br.DollarQuotation.Worker.Services;

public sealed class QuotationNotificationClient : IQuotationNotificationClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<QuotationNotificationClient> _logger;

    public QuotationNotificationClient(HttpClient httpClient,
        ILogger<QuotationNotificationClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task NotifyAsync( CurrencyQuotationResponse quotation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(quotation);

        using var response = await _httpClient.PostAsJsonAsync("api/internal/quotation-notifications", quotation, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("A API não aceitou a notificação da cotação {CurrencyPair}. Status: {StatusCode}.", quotation.CurrencyPair, (int)response.StatusCode);

            return;
        }

        _logger.LogInformation("Cotação {CurrencyPair} enviada para a API em tempo real.", quotation.CurrencyPair);
    }
}