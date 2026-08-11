using Br.DollarQuotation.API.Services.Interfaces;
using Br.DollarQuotation.Domain.Entities;
using Br.DollarQuotation.Domain.Interfaces.Repositories;
using Br.DollarQuotation.Messaging.Configurations;
using Br.DollarQuotation.Messaging.Contracts;
using Br.DollarQuotation.Messaging.Interfaces;
using Microsoft.Extensions.Options;

namespace Br.DollarQuotation.API.Services;

public sealed class QuotationUpdatedConsumerWorker : BackgroundService
{
    private readonly IMessageConsumer _messageConsumer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqOptions _rabbitMqOptions;
    private readonly ILogger<QuotationUpdatedConsumerWorker> _logger;

    public QuotationUpdatedConsumerWorker(
        IMessageConsumer messageConsumer,
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> rabbitMqOptions,
        ILogger<QuotationUpdatedConsumerWorker> logger)
    {
        _messageConsumer = messageConsumer;
        _scopeFactory = scopeFactory;
        _rabbitMqOptions = rabbitMqOptions.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        ValidateOptions();

        _logger.LogInformation(
            "Iniciando consumer de atualização de cotações. " +
            "Queue: {QueueName} | RoutingKey: {RoutingKey}.",
            _rabbitMqOptions.QuotationQueueName,
            _rabbitMqOptions.QuotationRoutingKey
        );

        try
        {
            await _messageConsumer.ConsumeAsync<QuotationUpdatedMessage>(
                _rabbitMqOptions.QuotationQueueName,
                _rabbitMqOptions.QuotationRoutingKey,
                ProcessMessageAsync,
                stoppingToken
            );
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Consumer de atualização de cotações finalizado."
            );
        }
        catch (Exception exception)
        {
            _logger.LogCritical(
                exception,
                "Erro crítico no consumer de atualização de cotações."
            );

            throw;
        }
    }

    private async Task ProcessMessageAsync(
        QuotationUpdatedMessage message,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processando cotação recebida do RabbitMQ: " +
            "{CurrencyPair} | Compra: {BidPrice} | Venda: {AskPrice}.",
            message.CurrencyPair,
            message.BidPrice,
            message.AskPrice
        );

        await using var scope =
            _scopeFactory.CreateAsyncScope();

        var notificationService =
            scope.ServiceProvider
                .GetRequiredService<IQuotationNotificationService>();

        await EvaluateQuotationAlertsAsync(
            scope.ServiceProvider,
            notificationService,
            message,
            cancellationToken
        );

        await notificationService.NotifyQuotationUpdatedAsync(
            message,
            cancellationToken
        );

        _logger.LogInformation(
            "Cotação {CurrencyPair} encaminhada para o SignalR.",
            message.CurrencyPair
        );
    }

    private async Task EvaluateQuotationAlertsAsync(
        IServiceProvider serviceProvider,
        IQuotationNotificationService notificationService,
        QuotationUpdatedMessage message,
        CancellationToken cancellationToken)
    {
        var quotationAlertRepository =
            serviceProvider.GetRequiredService<IQuotationAlertRepository>();

        var currencyPairCode =
            $"{message.BaseCurrency}-{message.QuoteCurrency}";

        var alerts =
            await quotationAlertRepository.GetActiveByCurrencyPairAsync(
                currencyPairCode,
                cancellationToken
            );

        if (alerts.Count == 0)
        {
            _logger.LogInformation(
                "Nenhum alerta ativo encontrado para {CurrencyPair}.",
                currencyPairCode
            );

            return;
        }

        _logger.LogInformation(
            "{AlertCount} alerta(s) ativo(s) encontrado(s) para {CurrencyPair}.",
            alerts.Count,
            currencyPairCode
        );

        var triggeredAlerts =
            new List<QuotationAlert>();

        foreach (var alert in alerts)
        {
            var shouldTrigger =
                alert.ShouldTrigger(
                    message.BidPrice
                );

            if (!shouldTrigger)
            {
                _logger.LogInformation(
                    "Alerta {AlertId} não atingido. " +
                    "Par: {CurrencyPair} | " +
                    "Preço atual: {CurrentPrice} | " +
                    "Preço alvo: {TargetPrice} | " +
                    "Condição: {Condition}.",
                    alert.Id,
                    currencyPairCode,
                    message.BidPrice,
                    alert.TargetPrice,
                    alert.Condition
                );

                continue;
            }

            alert.MarkAsTriggered();

            triggeredAlerts.Add(
                alert
            );

            _logger.LogInformation(
                "Alerta {AlertId} DISPARADO. " +
                "Usuário: {UserId} | " +
                "Par: {CurrencyPair} | " +
                "Preço atual: {CurrentPrice} | " +
                "Preço alvo: {TargetPrice} | " +
                "Condição: {Condition}.",
                alert.Id,
                alert.UserId,
                currencyPairCode,
                message.BidPrice,
                alert.TargetPrice,
                alert.Condition
            );
        }

        if (triggeredAlerts.Count == 0)
        {
            return;
        }

        await quotationAlertRepository.SaveChangesAsync(
            cancellationToken
        );

        _logger.LogInformation(
            "{TriggeredCount} alerta(s) disparado(s) e persistido(s) para {CurrencyPair}.",
            triggeredAlerts.Count,
            currencyPairCode
        );

        foreach (var alert in triggeredAlerts)
        {
            await notificationService.NotifyQuotationAlertTriggeredAsync(
                alert,
                message.BidPrice,
                cancellationToken
            );

            _logger.LogInformation(
                "Alerta {AlertId} encaminhado para o SignalR.",
                alert.Id
            );
        }
    }

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(
                _rabbitMqOptions.QuotationQueueName))
        {
            throw new InvalidOperationException(
                "A fila de cotações do RabbitMQ não foi configurada."
            );
        }

        if (string.IsNullOrWhiteSpace(
                _rabbitMqOptions.QuotationRoutingKey))
        {
            throw new InvalidOperationException(
                "A RoutingKey de cotações do RabbitMQ não foi configurada."
            );
        }
    }
}