namespace Br.DollarQuotation.Application.DTOs.Requests;

public sealed class GetCurrentQuotationRequest
{
    public string BaseCurrency { get; set; } = string.Empty;

    public string QuoteCurrency { get; set; } = string.Empty;
}