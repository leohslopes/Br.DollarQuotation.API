namespace Br.DollarQuotation.Messaging.Configurations;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string HostName { get; set; } = "localhost";

    public int Port { get; set; } = 5672;

    public string UserName { get; set; } = "guest";

    public string Password { get; set; } = "guest";

    public string VirtualHost { get; set; } = "/";

    public string ExchangeName { get; set; } =
        "dollarquotation.exchange";

    public string QuotationQueueName { get; set; } =
        "dollarquotation.quotation.queue";

    public string QuotationRoutingKey { get; set; } =
        "quotation.updated";
}