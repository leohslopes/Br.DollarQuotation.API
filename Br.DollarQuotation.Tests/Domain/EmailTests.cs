using Br.DollarQuotation.Domain.Exceptions;
using Br.DollarQuotation.Domain.ValueObjects;

namespace Br.DollarQuotation.Tests.Domain.ValueObjects;

public sealed class EmailTests
{
    [Fact]
    public void Create_WithValidEmail_ShouldCreateEmail()
    {
        var email = Email.Create("teste@email.com");

        Assert.NotNull(email);
        Assert.Equal("teste@email.com", email.Value);
    }

    [Fact]
    public void Create_WithUpperCaseEmail_ShouldNormalizeToLowerCase()
    {
        var email = Email.Create("TESTE@EMAIL.COM");

        Assert.Equal(
            "teste@email.com",
            email.Value);
    }

    [Fact]
    public void Create_WithSpaces_ShouldTrimValue()
    {
        var email = Email.Create(
            "  teste@email.com  ");

        Assert.Equal(
            "teste@email.com",
            email.Value);
    }

    [Fact]
    public void Create_WithEmptyEmail_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(
            () => Email.Create(""));

        Assert.Equal(
            "O e-mail é obrigatório.",
            exception.Message);
    }

    [Fact]
    public void Create_WithWhiteSpaceEmail_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(
            () => Email.Create("   "));

        Assert.Equal(
            "O e-mail é obrigatório.",
            exception.Message);
    }

    [Fact]
    public void Create_WithInvalidEmail_ShouldThrowDomainException()
    {
        var exception = Assert.Throws<DomainException>(
            () => Email.Create("email-invalido"));

        Assert.Equal(
            "O e-mail informado é inválido.",
            exception.Message);
    }

    [Theory]
    [InlineData("teste@")]
    [InlineData("@email.com")]
    [InlineData("teste@email")]
    [InlineData("teste email@email.com")]
    public void Create_WithInvalidFormats_ShouldThrowDomainException(
        string value)
    {
        var exception = Assert.Throws<DomainException>(
            () => Email.Create(value));

        Assert.Equal(
            "O e-mail informado é inválido.",
            exception.Message);
    }

    [Fact]
    public void Create_WithMoreThan200Characters_ShouldThrowDomainException()
    {
        var localPart = new string('a', 191);

        var value = $"{localPart}@email.com";

        var exception = Assert.Throws<DomainException>(
            () => Email.Create(value));

        Assert.Contains(
            "200 caracteres",
            exception.Message);
    }

    [Fact]
    public void Equals_WithSameEmail_ShouldReturnTrue()
    {
        var first = Email.Create(
            "teste@email.com");

        var second = Email.Create(
            "TESTE@EMAIL.COM");

        Assert.Equal(first, second);
        Assert.True(first.Equals(second));
        Assert.True(first == second);
        Assert.False(first != second);
    }

    [Fact]
    public void Equals_WithDifferentEmails_ShouldReturnFalse()
    {
        var first = Email.Create(
            "primeiro@email.com");

        var second = Email.Create(
            "segundo@email.com");

        Assert.NotEqual(first, second);
        Assert.False(first.Equals(second));
        Assert.False(first == second);
        Assert.True(first != second);
    }

    [Fact]
    public void GetHashCode_WithSameEmail_ShouldReturnSameHashCode()
    {
        var first = Email.Create(
            "teste@email.com");

        var second = Email.Create(
            "TESTE@EMAIL.COM");

        Assert.Equal(
            first.GetHashCode(),
            second.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldReturnEmailValue()
    {
        var email = Email.Create(
            "teste@email.com");

        var result = email.ToString();

        Assert.Equal(
            "teste@email.com",
            result);
    }

    [Fact]
    public void ImplicitConversionToString_ShouldReturnEmailValue()
    {
        var email = Email.Create(
            "teste@email.com");

        string value = email;

        Assert.Equal(
            "teste@email.com",
            value);
    }

    [Fact]
    public void ExplicitConversionFromString_ShouldCreateEmail()
    {
        var email =
            (Email)"TESTE@EMAIL.COM";

        Assert.Equal(
            "teste@email.com",
            email.Value);
    }
}