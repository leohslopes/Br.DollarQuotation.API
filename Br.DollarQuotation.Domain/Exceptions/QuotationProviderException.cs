using Br.DollarQuotation.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Br.DollarQuotation.Domain.Exceptions
{
    public sealed class QuotationProviderException : DomainException
    {
        public QuotationProviderException(CurrencyPair currencyPair, string message) : base($"Não foi possível obter a cotação de " + $"{currencyPair.ToDisplay()}. {message}")
        {

        }

        public QuotationProviderException( CurrencyPair currencyPair, string message, Exception innerException) : base($"Não foi possível obter a cotação de " + $"{currencyPair.ToDisplay()}. {message}", innerException)
        {

        }
    }
}
