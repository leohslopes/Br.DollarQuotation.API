using System.Text;
using System.Text.Json;
using Br.DollarQuotation.Messaging.Configurations;
using Br.DollarQuotation.Messaging.Interfaces;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Br.DollarQuotation.Messaging.Publishers;

public sealed class RabbitMqPublisher : IMessagePublisher
{
    private readonly RabbitMqOptions _options;

    public RabbitMqPublisher(
        IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;
    }

    public async Task PublishAsync<TMessage>(
        TMessage message,
        string routingKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (string.IsNullOrWhiteSpace(routingKey))
        {
            throw new ArgumentException(
                "A routing key é obrigatória.",
                nameof(routingKey));
        }

        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost
        };

        await using var connection =
            await factory.CreateConnectionAsync(
                cancellationToken);

        await using var channel =
            await connection.CreateChannelAsync(
                cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: _options.ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        var json =
            JsonSerializer.Serialize(
                message);

        var body =
            Encoding.UTF8.GetBytes(
                json);

        var properties =
            new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                ContentEncoding = "utf-8",
                MessageId = Guid.NewGuid().ToString(),
                Timestamp = new AmqpTimestamp(
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };

        await channel.BasicPublishAsync(
            exchange: _options.ExchangeName,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }
}
