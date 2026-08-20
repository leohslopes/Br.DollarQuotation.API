using Br.DollarQuotation.API.Services.Interfaces;
using Br.DollarQuotation.Domain.Entities;
using Br.DollarQuotation.Domain.Enums;
using Br.DollarQuotation.Domain.Interfaces.Repositories;
using Br.DollarQuotation.Domain.Interfaces.Services;
using Br.DollarQuotation.Messaging.Configurations;
using Br.DollarQuotation.Messaging.Contracts;
using Br.DollarQuotation.Messaging.Interfaces;
using Microsoft.Extensions.Options;

namespace Br.DollarQuotation.API.Services;

public sealed class QuotationUpdatedConsumerWorker
    : BackgroundService
{
    private const int InitialRetryDelaySeconds = 2;
    private const int MaximumRetryDelaySeconds = 30;

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
        _messageConsumer =
            messageConsumer;

        _scopeFactory =
            scopeFactory;

        _rabbitMqOptions =
            rabbitMqOptions.Value;

        _logger =
            logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        ValidateOptions();

        _logger.LogInformation(
            "Iniciando consumer de atualização de cotações. " +
            "Queue: {QueueName} | RoutingKey: {RoutingKey}.",
            _rabbitMqOptions.QuotationQueueName,
            _rabbitMqOptions.QuotationRoutingKey);

        var retryAttempt = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _messageConsumer
                    .ConsumeAsync<QuotationUpdatedMessage>(
                        _rabbitMqOptions.QuotationQueueName,
                        _rabbitMqOptions.QuotationRoutingKey,
                        ProcessMessageAsync,
                        stoppingToken);

                /*
                 * Se ConsumeAsync retornar normalmente sem que
                 * a aplicação esteja sendo encerrada, permitimos
                 * uma nova tentativa de inicialização do consumer.
                 */

                if (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogWarning(
                        "O consumer RabbitMQ foi encerrado inesperadamente. " +
                        "Uma nova conexão será iniciada.");

                    retryAttempt = 0;
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                retryAttempt++;

                var retryDelay =
                    CalculateRetryDelay(
                        retryAttempt);

                _logger.LogWarning(
                    exception,
                    "Não foi possível iniciar ou manter o consumer RabbitMQ. " +
                    "Tentativa: {RetryAttempt}. " +
                    "Nova tentativa em {RetryDelaySeconds} segundo(s).",
                    retryAttempt,
                    retryDelay.TotalSeconds);

                try
                {
                    await Task.Delay(
                        retryDelay,
                        stoppingToken);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        _logger.LogInformation(
            "Consumer de atualização de cotações finalizado.");
    }

    private static TimeSpan CalculateRetryDelay(
        int retryAttempt)
    {
        /*
         * Backoff:
         *
         * tentativa 1 -> 2s
         * tentativa 2 -> 4s
         * tentativa 3 -> 8s
         * tentativa 4 -> 16s
         * tentativa 5+ -> 30s
         */

        var exponent =
            Math.Min(
                retryAttempt - 1,
                4);

        var seconds =
            InitialRetryDelaySeconds *
            Math.Pow(
                2,
                exponent);

        seconds =
            Math.Min(
                seconds,
                MaximumRetryDelaySeconds);

        return TimeSpan.FromSeconds(
            seconds);
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
            message.AskPrice);

        await using var scope =
            _scopeFactory.CreateAsyncScope();

        var notificationService =
            scope.ServiceProvider
                .GetRequiredService<
                    IQuotationNotificationService>();

        await EvaluateQuotationAlertsAsync(
            scope.ServiceProvider,
            notificationService,
            message,
            cancellationToken);

        await notificationService
            .NotifyQuotationUpdatedAsync(
                message,
                cancellationToken);

        _logger.LogInformation(
            "Cotação {CurrencyPair} encaminhada para o SignalR.",
            message.CurrencyPair);
    }

    private async Task EvaluateQuotationAlertsAsync(
        IServiceProvider serviceProvider,
        IQuotationNotificationService notificationService,
        QuotationUpdatedMessage message,
        CancellationToken cancellationToken)
    {
        var quotationAlertRepository =
            serviceProvider
                .GetRequiredService<
                    IQuotationAlertRepository>();

        var userRepository =
            serviceProvider
                .GetRequiredService<
                    IUserRepository>();

        var emailService =
            serviceProvider
                .GetRequiredService<
                    IEmailService>();

        var currencyPairCode =
            $"{message.BaseCurrency}-{message.QuoteCurrency}";

        var alerts =
            await quotationAlertRepository
                .GetActiveByCurrencyPairAsync(
                    currencyPairCode,
                    cancellationToken);

        if (alerts.Count == 0)
        {
            _logger.LogInformation(
                "Nenhum alerta ativo encontrado para {CurrencyPair}.",
                currencyPairCode);

            return;
        }

        _logger.LogInformation(
            "{AlertCount} alerta(s) ativo(s) encontrado(s) para {CurrencyPair}.",
            alerts.Count,
            currencyPairCode);

        var triggeredAlerts =
            new List<QuotationAlert>();

        foreach (var alert in alerts)
        {
            var shouldTrigger =
                alert.ShouldTrigger(
                    message.BidPrice);

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
                    alert.Condition);

                continue;
            }

            alert.MarkAsTriggered();

            triggeredAlerts.Add(
                alert);

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
                alert.Condition);
        }

        if (triggeredAlerts.Count == 0)
        {
            return;
        }

        await quotationAlertRepository
            .SaveChangesAsync(
                cancellationToken);

        _logger.LogInformation(
            "{TriggeredCount} alerta(s) disparado(s) e persistido(s) para {CurrencyPair}.",
            triggeredAlerts.Count,
            currencyPairCode);

        foreach (var alert in triggeredAlerts)
        {
            await notificationService
                .NotifyQuotationAlertTriggeredAsync(
                    alert,
                    message.BidPrice,
                    cancellationToken);

            _logger.LogInformation(
                "Alerta {AlertId} encaminhado para o SignalR.",
                alert.Id);

            await SendQuotationAlertEmailAsync(
                userRepository,
                emailService,
                alert,
                currencyPairCode,
                message.BidPrice,
                cancellationToken);
        }
    }

    private async Task SendQuotationAlertEmailAsync(
        IUserRepository userRepository,
        IEmailService emailService,
        QuotationAlert alert,
        string currencyPair,
        decimal currentPrice,
        CancellationToken cancellationToken)
    {
        try
        {
            var user =
                await userRepository
                    .GetByIdAsync(
                        alert.UserId,
                        cancellationToken);

            if (user is null)
            {
                _logger.LogWarning(
                    "Não foi possível enviar o e-mail do alerta {AlertId}. " +
                    "Usuário {UserId} não encontrado.",
                    alert.Id,
                    alert.UserId);

                return;
            }

            if (!user.IsActive)
            {
                _logger.LogWarning(
                    "Não foi possível enviar o e-mail do alerta {AlertId}. " +
                    "Usuário {UserId} está inativo.",
                    alert.Id,
                    alert.UserId);

                return;
            }

            var conditionDescription =
                GetConditionDescription(
                    alert.Condition);

            await emailService
                .SendQuotationAlertTriggeredAsync(
                    user.Email.Value,
                    user.Name,
                    currencyPair,
                    currentPrice,
                    alert.TargetPrice,
                    conditionDescription,
                    cancellationToken);

            _logger.LogInformation(
                "E-mail do alerta {AlertId} enviado para {RecipientEmail}.",
                alert.Id,
                user.Email.Value);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            /*
             * Falha no e-mail não deve interromper
             * o processamento da cotação nem desfazer
             * o alerta já persistido.
             */

            _logger.LogError(
                exception,
                "Falha ao enviar e-mail referente ao alerta {AlertId}.",
                alert.Id);
        }
    }

    private static string GetConditionDescription(
        AlertCondition condition)
    {
        return condition switch
        {
            AlertCondition.Above =>
                "Acima ou igual ao preço-alvo",

            AlertCondition.Below =>
                "Abaixo ou igual ao preço-alvo",

            _ =>
                condition.ToString()
        };
    }

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(
            _rabbitMqOptions.QuotationQueueName))
        {
            throw new InvalidOperationException(
                "A fila de cotações do RabbitMQ não foi configurada.");
        }

        if (string.IsNullOrWhiteSpace(
            _rabbitMqOptions.QuotationRoutingKey))
        {
            throw new InvalidOperationException(
                "A RoutingKey de cotações do RabbitMQ não foi configurada.");
        }
    }
}