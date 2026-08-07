namespace Br.DollarQuotation.Domain.Models;

public sealed class CurrencyQuotationSummary
{
    public decimal MinimumBidPrice { get; set; }

    public decimal MaximumBidPrice { get; set; }

    public decimal AverageBidPrice { get; set; }

    public int TotalQuotations { get; set; }
}