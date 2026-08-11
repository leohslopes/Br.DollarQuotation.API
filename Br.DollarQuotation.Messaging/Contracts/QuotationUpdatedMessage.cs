namespace Br.DollarQuotation.Messaging.Contracts;

public sealed record QuotationUpdatedMessage
{
    public Guid Id { get; init; }

    public string BaseCurrency { get; init; } =
        string.Empty;

    public string QuoteCurrency { get; init; } =
        string.Empty;

    public string CurrencyPair { get; init; } =
        string.Empty;

    public decimal BidPrice { get; init; }

    public decimal AskPrice { get; init; }

    public decimal HighPrice { get; init; }

    public decimal LowPrice { get; init; }

    public decimal Variation { get; init; }

    public decimal VariationPercentage { get; init; }

    public DateTime QuotationDate { get; init; }

    public DateTime PublishedAt { get; init; } =
        DateTime.UtcNow;
}