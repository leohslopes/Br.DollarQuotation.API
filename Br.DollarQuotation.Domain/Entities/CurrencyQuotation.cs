using Br.DollarQuotation.Domain.Common;
using Br.DollarQuotation.Domain.Enums;
using Br.DollarQuotation.Domain.Exceptions;
using Br.DollarQuotation.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Br.DollarQuotation.Domain.Entities
{
    public class CurrencyQuotation : Entity
    {
        public CurrencyPair CurrencyPair { get; private set; } = null!;

        public decimal BidPrice { get; private set; }

        public decimal AskPrice { get; private set; }

        public decimal HighPrice { get; private set; }

        public decimal LowPrice { get; private set; }

        public decimal Variation { get; private set; }

        public decimal VariationPercentage { get; private set; }

        public DateTime QuotationDate { get; private set; }

        public DateTime CreatedAt { get; private set; }

        protected CurrencyQuotation()
        {
        }

        public CurrencyQuotation(
            CurrencyPair currencyPair,
            decimal bidPrice,
            decimal askPrice,
            decimal highPrice,
            decimal lowPrice,
            decimal variation,
            decimal variationPercentage,
            DateTime quotationDate)
        {
            SetCurrencyPair(currencyPair);

            SetPrices(
                bidPrice,
                askPrice,
                highPrice,
                lowPrice);

            Variation = variation;
            VariationPercentage = variationPercentage;
            QuotationDate = ValidateQuotationDate(quotationDate);
            CreatedAt = DateTime.UtcNow;
        }

        public void UpdatePrices(
            decimal bidPrice,
            decimal askPrice,
            decimal highPrice,
            decimal lowPrice,
            decimal variation,
            decimal variationPercentage,
            DateTime quotationDate)
        {
            SetPrices(
                bidPrice,
                askPrice,
                highPrice,
                lowPrice);

            Variation = variation;
            VariationPercentage = variationPercentage;
            QuotationDate = ValidateQuotationDate(quotationDate);
        }

        private void SetCurrencyPair(CurrencyPair currencyPair)
        {
            CurrencyPair = currencyPair ?? throw new DomainException("O par de moedas é obrigatório.");
        }

        private void SetPrices(
            decimal bidPrice,
            decimal askPrice,
            decimal highPrice,
            decimal lowPrice)
        {
            if (bidPrice <= 0)
            {
                throw new DomainException( "O preço de compra deve ser maior que zero.");
            }

            if (askPrice <= 0)
            {
                throw new DomainException("O preço de venda deve ser maior que zero.");
            }

            if (highPrice <= 0)
            {
                throw new DomainException("A maior cotação deve ser maior que zero.");
            }

            if (lowPrice <= 0)
            {
                throw new DomainException("A menor cotação deve ser maior que zero.");
            }

            if (lowPrice > highPrice)
            {
                throw new DomainException("A menor cotação não pode ser maior que a maior cotação.");
            }

            BidPrice = bidPrice;
            AskPrice = askPrice;
            HighPrice = highPrice;
            LowPrice = lowPrice;
        }

        private static DateTime ValidateQuotationDate(
            DateTime quotationDate)
        {
            if (quotationDate == default)
            {
                throw new DomainException("A data da cotação é obrigatória.");
            }

            return quotationDate.Kind == DateTimeKind.Utc ? quotationDate : quotationDate.ToUniversalTime();
        }
    }

}
