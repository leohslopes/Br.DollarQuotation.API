using Br.DollarQuotation.Domain.Enums;

namespace Br.DollarQuotation.Application.DTOs.Requests;

public sealed class CreateQuotationAlertRequest
{
    public string BaseCurrency { get; set; } = string.Empty;

    public string QuoteCurrency { get; set; } = string.Empty;

    public AlertCondition Condition { get; set; }

    public decimal TargetPrice { get; set; }
}