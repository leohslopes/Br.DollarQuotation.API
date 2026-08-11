using Br.DollarQuotation.Application.DTOs.Requests;
using Br.DollarQuotation.Application.Interfaces.Services;
using Br.DollarQuotation.Messaging.Configurations;
using Br.DollarQuotation.Messaging.Contracts;
using Br.DollarQuotation.Messaging.Interfaces;
using Br.DollarQuotation.Worker.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Br.DollarQuotation.Worker.Services;

public sealed class CurrencyQuotationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CurrencyQuotationWorker> _logger;
    private readonly QuotationWorkerOptions _options;
    private readonly IMessagePublisher _messagePublisher;
    private readonly RabbitMqOptions _rabbitMqOptions;

    public CurrencyQuotationWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<CurrencyQuotationWorker> logger,
        IOptions<QuotationWorkerOptions> options,
        IMessagePublisher messagePublisher,
        IOptions<RabbitMqOptions> rabbitMqOptions)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
        _messagePublisher = messagePublisher;
        _rabbitMqOptions = rabbitMqOptions.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("O Worker de cotações está desativado.");

            return;
        }

        ValidateOptions();

        _logger.LogInformation("Worker de cotações iniciado. Intervalo: {IntervalInSeconds} segundos.", _options.IntervalInSeconds);

        await ExecuteSynchronizationSafelyAsync(stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.IntervalInSeconds));

        try
        {
            while (
                await timer.WaitForNextTickAsync(stoppingToken))
            {
                await ExecuteSynchronizationSafelyAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("O Worker de cotações foi finalizado corretamente.");
        }
    }

    private async Task ExecuteSynchronizationSafelyAsync(CancellationToken cancellationToken)
    {
        try
        {
            await SynchronizeQuotationsAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("A sincronização das cotações foi cancelada.");
        }
        catch (Exception exception)
        {
            _logger.LogCritical(exception, "Ocorreu um erro inesperado durante o ciclo de sincronização.");
        }
    }

    private async Task SynchronizeQuotationsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando sincronização das cotações em {ExecutionDate}.", DateTime.UtcNow);

        foreach (var currencyPairCode in _options.CurrencyPairs)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            await SynchronizeCurrencyPairAsync(currencyPairCode,cancellationToken);
            await DelayBetweenRequestsAsync(cancellationToken);
        }

        _logger.LogInformation("Sincronização das cotações finalizada em {ExecutionDate}.", DateTime.UtcNow);
    }

    private async Task SynchronizeCurrencyPairAsync(string currencyPairCode, CancellationToken cancellationToken)
    {
        try
        {
            var currencies =ParseCurrencyPair(currencyPairCode);

            await using var scope = _scopeFactory.CreateAsyncScope();

            var quotationService = scope.ServiceProvider.GetRequiredService<ICurrencyQuotationService>();

            var response =
                await quotationService
                    .GetCurrentAsync(
                        new GetCurrentQuotationRequest
                        {
                            BaseCurrency =
                                currencies.BaseCurrency,

                            QuoteCurrency =
                                currencies.QuoteCurrency
                        },
                        cancellationToken);

            if (response.WasInserted)
            {
                _logger.LogInformation(
                    "Nova cotação salva: {CurrencyPair} | " +
                    "Compra: {BidPrice} | " +
                    "Venda: {AskPrice} | " +
                    "Data: {QuotationDate}.",
                    response.CurrencyPair,
                    response.BidPrice,
                    response.AskPrice,
                    response.QuotationDate);

                var message =
                    new QuotationUpdatedMessage
                    {
                        Id =
                            response.Id,

                        BaseCurrency =
                            response.BaseCurrency,

                        QuoteCurrency =
                            response.QuoteCurrency,

                        CurrencyPair =
                            response.CurrencyPair,

                        BidPrice =
                            response.BidPrice,

                        AskPrice =
                            response.AskPrice,

                        HighPrice =
                            response.HighPrice,

                        LowPrice =
                            response.LowPrice,

                        Variation =
                            response.Variation,

                        VariationPercentage =
                            response.VariationPercentage,

                        QuotationDate =
                            response.QuotationDate,

                        PublishedAt =
                            DateTime.UtcNow
                    };

                await _messagePublisher
                    .PublishAsync(
                        message,
                        _rabbitMqOptions
                            .QuotationRoutingKey,
                        cancellationToken);

                _logger.LogInformation(
                    "Cotação {CurrencyPair} publicada no RabbitMQ. " +
                    "RoutingKey: {RoutingKey}.",
                    message.CurrencyPair,
                    _rabbitMqOptions
                        .QuotationRoutingKey);
            }
            else
            {
                _logger.LogInformation(
                    "Cotação já existente: {CurrencyPair} | " +
                    "Data: {QuotationDate}.",
                    response.CurrencyPair,
                    response.QuotationDate);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Sincronização do par {CurrencyPair} cancelada.",currencyPairCode);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Erro ao sincronizar o par de moedas {CurrencyPair}.", currencyPairCode);
        }
    }

    private async Task DelayBetweenRequestsAsync(CancellationToken cancellationToken)
    {
        if (_options.DelayBetweenRequestsInMilliseconds <= 0)
        {
            return;
        }

        await Task.Delay(TimeSpan.FromMilliseconds(_options.DelayBetweenRequestsInMilliseconds), cancellationToken);
    }

    private void ValidateOptions()
    {
        if (_options.IntervalInSeconds <= 0)
        {
            throw new InvalidOperationException( "O intervalo do Worker deve ser maior que zero.");
        }

        if (_options.DelayBetweenRequestsInMilliseconds < 0)
        {
            throw new InvalidOperationException("O intervalo entre as requisições não pode ser negativo.");
        }

        if (_options.CurrencyPairs is null || _options.CurrencyPairs.Count == 0)
        {
            throw new InvalidOperationException("Nenhum par de moedas foi configurado para o Worker.");
        }

        if (string.IsNullOrWhiteSpace(_rabbitMqOptions.QuotationRoutingKey))
        {
            throw new InvalidOperationException("A RoutingKey de cotações do RabbitMQ não foi configurada.");
        }
    }

    private static (string BaseCurrency, string QuoteCurrency) ParseCurrencyPair(string currencyPairCode)
    {
        if (string.IsNullOrWhiteSpace(currencyPairCode))
        {
            throw new InvalidOperationException("O par de moedas não pode ser vazio.");
        }

        var currencies = currencyPairCode.Trim().Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (currencies.Length != 2)
        {
            throw new InvalidOperationException($"O par de moedas '{currencyPairCode}' é inválido. " + "Utilize o formato USD-BRL.");
        }

        return (currencies[0].ToUpperInvariant(), currencies[1].ToUpperInvariant());
    }
}