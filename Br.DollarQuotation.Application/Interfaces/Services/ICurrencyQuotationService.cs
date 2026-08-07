using Br.DollarQuotation.Application.DTOs.Requests;
using Br.DollarQuotation.Application.DTOs.Responses;

namespace Br.DollarQuotation.Application.Interfaces.Services;

public interface ICurrencyQuotationService
{
    Task<CurrencyQuotationResponse> GetCurrentAsync(GetCurrentQuotationRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CurrencyQuotationResponse>> GetHistoryAsync(GetQuotationHistoryRequest request,CancellationToken cancellationToken = default);

    Task<PagedResponse<CurrencyQuotationResponse>> GetPagedAsync(GetQuotationPagedRequest request, CancellationToken cancellationToken = default);

    Task<CurrencyQuotationSummaryResponse> GetSummaryAsync(GetQuotationSummaryRequest request, CancellationToken cancellationToken = default);
}

