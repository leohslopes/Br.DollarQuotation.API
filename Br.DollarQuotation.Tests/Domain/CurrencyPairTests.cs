using Br.DollarQuotation.Domain.Enums;
using Br.DollarQuotation.Domain.Exceptions;
using Br.DollarQuotation.Domain.ValueObjects;

namespace Br.DollarQuotation.Tests.Domain;

public sealed class CurrencyPairTests
{
    [Fact]
    public void Create_WithValidCurrencies_ShouldCreateCurrencyPair()
    {
        var currencyPair = CurrencyPair.Create(
            CurrencyType.USD,
            CurrencyType.BRL);

        Assert.NotNull(currencyPair);
        Assert.Equal(CurrencyType.USD, currencyPair.BaseCurrency);
        Assert.Equal(CurrencyType.BRL, currencyPair.QuoteCurrency);
    }

    [Fact]
    public void Create_WithValidStrings_ShouldCreateCurrencyPair()
    {
        var currencyPair = CurrencyPair.Create(
            "USD",
            "BRL");

        Assert.Equal(CurrencyType.USD, currencyPair.BaseCurrency);
        Assert.Equal(CurrencyType.BRL, currencyPair.QuoteCurrency);
    }

    [Fact]
    public void Create_WithLowerCaseCurrencies_ShouldCreateCurrencyPair()
    {
        var currencyPair = CurrencyPair.Create(
            "usd",
            "brl");

        Assert.Equal(CurrencyType.USD, currencyPair.BaseCurrency);
        Assert.Equal(CurrencyType.BRL, currencyPair.QuoteCurrency);
    }

    [Fact]
    public void Create_WithSameCurrencies_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(
            () => CurrencyPair.Create(
                CurrencyType.USD,
                CurrencyType.USD));

        Assert.Equal(
            "A moeda base não pode ser igual à moeda de cotação.",
            exception.Message);
    }

    [Fact]
    public void Create_WithInvalidBaseCurrency_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(
            () => CurrencyPair.Create(
                "ABC",
                "BRL"));

        Assert.Contains(
            "A moeda base",
            exception.Message);
    }

    [Fact]
    public void Create_WithInvalidQuoteCurrency_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(
            () => CurrencyPair.Create(
                "USD",
                "ABC"));

        Assert.Contains(
            "A moeda de cotação",
            exception.Message);
    }

    [Fact]
    public void ToCode_ShouldReturnDashSeparatedCurrencies()
    {
        var currencyPair = CurrencyPair.Create(
            CurrencyType.USD,
            CurrencyType.BRL);

        var result = currencyPair.ToCode();

        Assert.Equal("USD-BRL", result);
    }

    [Fact]
    public void ToDisplay_ShouldReturnSlashSeparatedCurrencies()
    {
        var currencyPair = CurrencyPair.Create(
            CurrencyType.USD,
            CurrencyType.BRL);

        var result = currencyPair.ToDisplay();

        Assert.Equal("USD/BRL", result);
    }

    [Fact]
    public void Equals_WithSameCurrencies_ShouldReturnTrue()
    {
        var first = CurrencyPair.Create(
            CurrencyType.USD,
            CurrencyType.BRL);

        var second = CurrencyPair.Create(
            CurrencyType.USD,
            CurrencyType.BRL);

        Assert.Equal(first, second);
        Assert.True(first == second);
    }

    [Fact]
    public void Equals_WithDifferentCurrencies_ShouldReturnFalse()
    {
        var first = CurrencyPair.Create(
            CurrencyType.USD,
            CurrencyType.BRL);

        var second = CurrencyPair.Create(
            CurrencyType.EUR,
            CurrencyType.BRL);

        Assert.NotEqual(first, second);
        Assert.True(first != second);
    }
}