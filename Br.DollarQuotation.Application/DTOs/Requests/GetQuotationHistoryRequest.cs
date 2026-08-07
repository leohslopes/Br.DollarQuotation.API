namespace Br.DollarQuotation.Application.DTOs.Requests;

public sealed class GetQuotationHistoryRequest
{
    public string BaseCurrency { get; set; } = string.Empty;

    public string QuoteCurrency { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }
}
