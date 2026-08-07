namespace Br.DollarQuotation.Application.DTOs.Requests;

public sealed class GetQuotationPagedRequest
{
    public string? BaseCurrency { get; set; }

    public string? QuoteCurrency { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}