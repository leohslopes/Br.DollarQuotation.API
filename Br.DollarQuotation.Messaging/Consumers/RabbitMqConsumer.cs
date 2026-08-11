using System.Text;
using System.Text.Json;
using Br.DollarQuotation.Messaging.Configurations;
using Br.DollarQuotation.Messaging.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Br.DollarQuotation.Messaging.Consumers;

public sealed class RabbitMqConsumer : IMessageConsumer
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqConsumer> _logger;

    public RabbitMqConsumer(
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqConsumer> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task ConsumeAsync<TMessage>(
        string queueName,
        string routingKey,
        Func<TMessage, CancellationToken, Task> messageHandler,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(queueName))
        {
            throw new ArgumentException(
                "O nome da fila é obrigatório.",
                nameof(queueName));
        }

        if (string.IsNullOrWhiteSpace(routingKey))
        {
            throw new ArgumentException(
                "A RoutingKey é obrigatória.",
                nameof(routingKey));
        }

        ArgumentNullException.ThrowIfNull(
            messageHandler);

        ValidateOptions();

        var factory =
            new ConnectionFactory
            {
                HostName =
                    _options.HostName,

                Port =
                    _options.Port,

                UserName =
                    _options.UserName,

                Password =
                    _options.Password,

                VirtualHost =
                    _options.VirtualHost,

                AutomaticRecoveryEnabled =
                    true,

                NetworkRecoveryInterval =
                    TimeSpan.FromSeconds(5)
            };

        await using var connection =
            await factory.CreateConnectionAsync(
                cancellationToken);

        await using var channel =
            await connection.CreateChannelAsync(
                cancellationToken:
                    cancellationToken);

        // =============================
        // EXCHANGE
        // =============================

        await channel.ExchangeDeclareAsync(
            exchange:
                _options.ExchangeName,

            type:
                ExchangeType.Topic,

            durable:
                true,

            autoDelete:
                false,

            arguments:
                null,

            cancellationToken:
                cancellationToken);

        // =============================
        // FILA
        // =============================

        await channel.QueueDeclareAsync(
            queue:
                queueName,

            durable:
                true,

            exclusive:
                false,

            autoDelete:
                false,

            arguments:
                null,

            cancellationToken:
                cancellationToken);

        // =============================
        // BIND
        // =============================

        await channel.QueueBindAsync(
            queue:
                queueName,

            exchange:
                _options.ExchangeName,

            routingKey:
                routingKey,

            arguments:
                null,

            cancellationToken:
                cancellationToken);

        _logger.LogInformation(
            "Consumer RabbitMQ iniciado. " +
            "Queue: {QueueName} | " +
            "Exchange: {ExchangeName} | " +
            "RoutingKey: {RoutingKey}.",
            queueName,
            _options.ExchangeName,
            routingKey);

        // =============================
        // CONSUMER
        // =============================

        var consumer =
            new AsyncEventingBasicConsumer(
                channel);

        consumer.ReceivedAsync +=
            async (_, eventArgs) =>
            {
                try
                {
                    var body =
                        eventArgs.Body
                            .ToArray();

                    var json =
                        Encoding.UTF8
                            .GetString(body);

                    var message =
                        JsonSerializer
                            .Deserialize<TMessage>(
                                json);

                    if (message is null)
                    {
                        throw new InvalidOperationException(
                            "Não foi possível desserializar a mensagem recebida do RabbitMQ.");
                    }

                    _logger.LogInformation(
                        "Mensagem recebida. " +
                        "Queue: {QueueName} | " +
                        "RoutingKey: {RoutingKey} | " +
                        "DeliveryTag: {DeliveryTag}.",
                        queueName,
                        eventArgs.RoutingKey,
                        eventArgs.DeliveryTag);

                    await messageHandler(
                        message,
                        cancellationToken);

                    // =====================
                    // ACK
                    // =====================

                    await channel.BasicAckAsync(
                        deliveryTag:
                            eventArgs.DeliveryTag,

                        multiple:
                            false,

                        cancellationToken:
                            cancellationToken);

                    _logger.LogInformation(
                        "Mensagem processada com sucesso. " +
                        "DeliveryTag: {DeliveryTag}.",
                        eventArgs.DeliveryTag);
                }
                catch (OperationCanceledException)
                    when (
                        cancellationToken
                            .IsCancellationRequested)
                {
                    _logger.LogInformation(
                        "Processamento da mensagem cancelado.");
                }
                catch (Exception exception)
                {
                    _logger.LogError(
                        exception,
                        "Erro ao processar mensagem do RabbitMQ. " +
                        "DeliveryTag: {DeliveryTag}.",
                        eventArgs.DeliveryTag);

                    // =====================
                    // NACK
                    // =====================

                    await channel.BasicNackAsync(
                        deliveryTag:
                            eventArgs.DeliveryTag,

                        multiple:
                            false,

                        requeue:
                            true,

                        cancellationToken:
                            CancellationToken.None);
                }
            };

        // =============================
        // INICIAR CONSUMO
        // =============================

        await channel.BasicConsumeAsync(
            queue:
                queueName,

            autoAck:
                false,

            consumer:
                consumer,

            cancellationToken:
                cancellationToken);

        try
        {
            await Task.Delay(
                Timeout.InfiniteTimeSpan,
                cancellationToken);
        }
        catch (OperationCanceledException)
            when (
                cancellationToken
                    .IsCancellationRequested)
        {
            _logger.LogInformation(
                "Consumer RabbitMQ finalizado. Queue: {QueueName}.",
                queueName);
        }
    }

    private void ValidateOptions()
    {
        if (
            string.IsNullOrWhiteSpace(
                _options.HostName))
        {
            throw new InvalidOperationException(
                "O HostName do RabbitMQ não foi configurado.");
        }

        if (_options.Port <= 0)
        {
            throw new InvalidOperationException(
                "A porta do RabbitMQ é inválida.");
        }

        if (
            string.IsNullOrWhiteSpace(
                _options.UserName))
        {
            throw new InvalidOperationException(
                "O usuário do RabbitMQ não foi configurado.");
        }

        if (
            string.IsNullOrWhiteSpace(
                _options.Password))
        {
            throw new InvalidOperationException(
                "A senha do RabbitMQ não foi configurada.");
        }

        if (
            string.IsNullOrWhiteSpace(
                _options.ExchangeName))
        {
            throw new InvalidOperationException(
                "O Exchange do RabbitMQ não foi configurado.");
        }
    }
}
