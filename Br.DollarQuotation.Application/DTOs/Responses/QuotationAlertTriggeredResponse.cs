using Br.DollarQuotation.Domain.Enums;

namespace Br.DollarQuotation.Application.DTOs.Responses;

public sealed class QuotationAlertTriggeredResponse
{
    public Guid AlertId { get; set; }

    public Guid UserId { get; set; }

    public string BaseCurrency { get; set; } =
        string.Empty;

    public string QuoteCurrency { get; set; } =
        string.Empty;

    public string CurrencyPair { get; set; } =
        string.Empty;

    public AlertCondition Condition { get; set; }

    public decimal TargetPrice { get; set; }

    public decimal CurrentPrice { get; set; }

    public DateTime TriggeredAt { get; set; }
}