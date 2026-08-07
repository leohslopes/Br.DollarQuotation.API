using Br.DollarQuotation.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Br.DollarQuotation.Domain.Exceptions
{
    public sealed class QuotationNotFoundException : DomainException
    {
        public QuotationNotFoundException(CurrencyPair currencyPair) : base($"Nenhuma cotação foi encontrada para o par {currencyPair.ToDisplay()}.")
        {

        }

        public QuotationNotFoundException(Guid id) : base($"A cotação com o identificador '{id}' não foi encontrada.")
        {

        }
    }
}
