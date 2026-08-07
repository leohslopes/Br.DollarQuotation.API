using Br.DollarQuotation.Domain.Entities;
using Br.DollarQuotation.Domain.Models;
using Br.DollarQuotation.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Br.DollarQuotation.Domain.Interfaces.Repositories
{
    public interface ICurrencyQuotationRepository
    {
        Task<CurrencyQuotation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<CurrencyQuotation?> GetLatestAsync(CurrencyPair currencyPair, CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<CurrencyQuotation>> GetHistoryAsync(CurrencyPair currencyPair, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

        Task AddAsync(CurrencyQuotation quotation, CancellationToken cancellationToken = default);

        Task AddRangeAsync(IEnumerable<CurrencyQuotation> quotations, CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(CurrencyPair currencyPair, DateTime quotationDate, CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<CurrencyQuotation>> GetPagedAsync(CurrencyPair? currencyPair, DateTime? startDate, DateTime? endDate, int page, int pageSize, CancellationToken cancellationToken = default);

        Task<int> CountAsync(CurrencyPair? currencyPair, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);

        Task<CurrencyQuotationSummary?> GetSummaryAsync(CurrencyPair currencyPair, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);

        Task<CurrencyQuotation?> GetFirstAsync(CurrencyPair currencyPair, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
    }
}
