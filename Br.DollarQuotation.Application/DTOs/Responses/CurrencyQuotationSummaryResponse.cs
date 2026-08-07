namespace Br.DollarQuotation.Application.DTOs.Responses;

public sealed class CurrencyQuotationSummaryResponse
{
    public string BaseCurrency { get; set; } = string.Empty;

    public string QuoteCurrency { get; set; } = string.Empty;

    public string CurrencyPair { get; set; } = string.Empty;

    public decimal LatestBidPrice { get; set; }

    public decimal LatestAskPrice { get; set; }

    public decimal MinimumBidPrice { get; set; }

    public decimal MaximumBidPrice { get; set; }

    public decimal AverageBidPrice { get; set; }

    public decimal VariationPercentage { get; set; }

    public DateTime LatestQuotationDate { get; set; }

    public int TotalQuotations { get; set; }
}