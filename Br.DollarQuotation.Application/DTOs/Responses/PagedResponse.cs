namespace Br.DollarQuotation.Application.DTOs.Responses;

public sealed class PagedResponse<T>
{
    public IReadOnlyCollection<T> Items { get; set; } = [];

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalItems { get; set; }

    public int TotalPages { get; set; }
}