using Br.DollarQuotation.API.Hubs;
using Br.DollarQuotation.API.Hubs.Clients;
using Br.DollarQuotation.API.Services.Interfaces;
using Br.DollarQuotation.Application.DTOs.Responses;
using Br.DollarQuotation.Domain.Entities;
using Br.DollarQuotation.Messaging.Contracts;
using Microsoft.AspNetCore.SignalR;

namespace Br.DollarQuotation.API.Services;

public sealed class QuotationNotificationService
    : IQuotationNotificationService
{
    private readonly IHubContext<
        QuotationHub,
        IQuotationHubClient> _hubContext;

    private readonly ILogger<
        QuotationNotificationService> _logger;

    public QuotationNotificationService(
        IHubContext<QuotationHub, IQuotationHubClient> hubContext,
        ILogger<QuotationNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyQuotationUpdatedAsync(
        QuotationUpdatedMessage quotation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            quotation
        );

        var response = new CurrencyQuotationResponse
        {
            Id = quotation.Id,
            BaseCurrency = quotation.BaseCurrency,
            QuoteCurrency = quotation.QuoteCurrency,
            CurrencyPair = quotation.CurrencyPair,
            BidPrice = quotation.BidPrice,
            AskPrice = quotation.AskPrice,
            HighPrice = quotation.HighPrice,
            LowPrice = quotation.LowPrice,
            Variation = quotation.Variation,
            VariationPercentage = quotation.VariationPercentage,
            QuotationDate = quotation.QuotationDate,
            WasInserted = true
        };

        await _hubContext
            .Clients
            .All
            .QuotationUpdated(
                response
            );

        _logger.LogInformation(
            "Cotação {CurrencyPair} enviada aos clientes conectados via SignalR.",
            quotation.CurrencyPair
        );
    }

    public async Task NotifyQuotationAlertTriggeredAsync(
        QuotationAlert alert,
        decimal currentPrice,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            alert
        );

        if (currentPrice <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(currentPrice),
                "O preço atual deve ser maior que zero."
            );
        }

        var response = new QuotationAlertTriggeredResponse
        {
            AlertId = alert.Id,
            UserId = alert.UserId,

            BaseCurrency =
                alert.CurrencyPair.BaseCurrency.ToString(),

            QuoteCurrency =
                alert.CurrencyPair.QuoteCurrency.ToString(),

            CurrencyPair =
                alert.CurrencyPair.ToCode(),

            Condition =
                alert.Condition,

            TargetPrice =
                alert.TargetPrice,

            CurrentPrice =
                currentPrice,

            TriggeredAt =
                alert.TriggeredAt
                ?? DateTime.UtcNow
        };

        var userId =
            alert.UserId.ToString();

        await _hubContext
            .Clients
            .User(
                userId
            )
            .QuotationAlertTriggered(
                response
            );

        _logger.LogInformation(
            "Alerta {AlertId} de {CurrencyPair} enviado via SignalR " +
            "para o usuário {UserId}. " +
            "Preço atual: {CurrentPrice} | Preço alvo: {TargetPrice}.",
            alert.Id,
            response.CurrencyPair,
            userId,
            currentPrice,
            alert.TargetPrice
        );
    }
}