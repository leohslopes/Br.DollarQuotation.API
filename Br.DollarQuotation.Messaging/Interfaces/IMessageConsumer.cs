namespace Br.DollarQuotation.Messaging.Interfaces;

public interface IMessageConsumer
{
    Task ConsumeAsync<TMessage>(string queueName, string routingKey,Func<TMessage, CancellationToken, Task> messageHandler, CancellationToken cancellationToken = default);
}
