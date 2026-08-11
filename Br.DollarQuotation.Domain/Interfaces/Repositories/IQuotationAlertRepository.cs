using Br.DollarQuotation.Domain.Entities;

namespace Br.DollarQuotation.Domain.Interfaces.Repositories;

public interface IQuotationAlertRepository
{
    Task AddAsync(QuotationAlert alert,CancellationToken cancellationToken = default);

    Task<QuotationAlert?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<QuotationAlert>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<QuotationAlert>> GetActiveByCurrencyPairAsync(string currencyPair, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}