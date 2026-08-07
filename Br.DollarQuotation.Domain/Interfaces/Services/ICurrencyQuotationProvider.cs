using Br.DollarQuotation.Domain.Entities;
using Br.DollarQuotation.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Br.DollarQuotation.Domain.Interfaces.Services
{
    public interface ICurrencyQuotationProvider
    {
        Task<CurrencyQuotation> GetCurrentAsync(CurrencyPair currencyPair, CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<CurrencyQuotation>> GetHistoryAsync(CurrencyPair currencyPair, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    }
}
