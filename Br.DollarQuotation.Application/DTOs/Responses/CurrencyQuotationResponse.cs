namespace Br.DollarQuotation.Application.DTOs.Responses;

public sealed class CurrencyQuotationResponse
{
    public Guid Id { get; set; }

    public string BaseCurrency { get; set; } = string.Empty;

    public string QuoteCurrency { get; set; } = string.Empty;

    public string CurrencyPair { get; set; } = string.Empty;

    public decimal BidPrice { get; set; }

    public decimal AskPrice { get; set; }

    public decimal HighPrice { get; set; }

    public decimal LowPrice { get; set; }

    public decimal Variation { get; set; }

    public decimal VariationPercentage { get; set; }

    public DateTime QuotationDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool WasInserted { get; set; }
}