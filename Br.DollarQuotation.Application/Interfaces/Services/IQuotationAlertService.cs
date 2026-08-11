using Br.DollarQuotation.Application.DTOs.Requests;
using Br.DollarQuotation.Application.DTOs.Responses;

namespace Br.DollarQuotation.Application.Interfaces.Services;

public interface IQuotationAlertService
{
    Task<QuotationAlertResponse> CreateAsync(Guid userId, CreateQuotationAlertRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<QuotationAlertResponse>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<QuotationAlertResponse> ActivateAsync(Guid userId, Guid alertId, CancellationToken cancellationToken = default);

    Task<QuotationAlertResponse> DeactivateAsync(Guid userId, Guid alertId,CancellationToken cancellationToken = default);
}