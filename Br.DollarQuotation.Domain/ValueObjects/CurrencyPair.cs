using Br.DollarQuotation.Domain.Enums;
using Br.DollarQuotation.Domain.Exceptions;

namespace Br.DollarQuotation.Domain.ValueObjects;

public sealed class CurrencyPair : IEquatable<CurrencyPair>
{
    public CurrencyType BaseCurrency { get; }

    public CurrencyType QuoteCurrency { get; }

    private CurrencyPair(
        CurrencyType baseCurrency,
        CurrencyType quoteCurrency)
    {
        BaseCurrency = baseCurrency;
        QuoteCurrency = quoteCurrency;
    }

    public static CurrencyPair Create(
        CurrencyType baseCurrency,
        CurrencyType quoteCurrency)
    {
        ValidateCurrency(baseCurrency, "A moeda base informada é inválida.");

        ValidateCurrency(quoteCurrency, "A moeda de cotação informada é inválida.");

        if (baseCurrency == quoteCurrency)
        {
            throw new DomainException("A moeda base não pode ser igual à moeda de cotação.");
        }

        return new CurrencyPair(baseCurrency, quoteCurrency);
    }

    public static CurrencyPair Create(
        string baseCurrency,
        string quoteCurrency)
    {
        if (!Enum.TryParse<CurrencyType>( baseCurrency, ignoreCase: true, out var parsedBaseCurrency))
        {
            throw new DomainException( $"A moeda base '{baseCurrency}' é inválida.");
        }

        if (!Enum.TryParse<CurrencyType>(quoteCurrency, ignoreCase: true, out var parsedQuoteCurrency))
        {
            throw new DomainException($"A moeda de cotação '{quoteCurrency}' é inválida.");
        }

        return Create(parsedBaseCurrency, parsedQuoteCurrency);
    }

    public static CurrencyPair FromCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException( "O código do par de moedas é obrigatório.");
        }

        var currencies = code.Trim().Split('-',StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (currencies.Length != 2)
        {
            throw new DomainException($"O código do par de moedas '{code}' é inválido.");
        }

        return Create(currencies[0],currencies[1]);
    }

    public string ToCode()
    {
        return $"{BaseCurrency}-{QuoteCurrency}";
    }

    public string ToDisplay()
    {
        return $"{BaseCurrency}/{QuoteCurrency}";
    }

    public override string ToString()
    {
        return ToDisplay();
    }

    public bool Equals(CurrencyPair? other)
    {
        if (other is null)
            return false;

        return BaseCurrency == other.BaseCurrency && QuoteCurrency == other.QuoteCurrency;
    }

    public override bool Equals(object? obj)
    {
        return obj is CurrencyPair other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(BaseCurrency, QuoteCurrency);
    }

    public static bool operator == (
        CurrencyPair? left,
        CurrencyPair? right)
    {
        return Equals(left, right);
    }

    public static bool operator != (
        CurrencyPair? left,
        CurrencyPair? right)
    {
        return !Equals(left, right);
    }

    private static void ValidateCurrency(
        CurrencyType currency,
        string errorMessage)
    {
        if (!Enum.IsDefined(currency))
        {
            throw new DomainException(errorMessage);
        }
    }
}