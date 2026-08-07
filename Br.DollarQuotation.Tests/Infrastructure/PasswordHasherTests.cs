using Br.DollarQuotation.Repository.Services;

namespace Br.DollarQuotation.Tests.Repository.Services;

public sealed class PasswordHasherTests
{
    private readonly PasswordHasher _passwordHasher;

    public PasswordHasherTests()
    {
        _passwordHasher = new PasswordHasher();
    }

    [Fact]
    public void Hash_WithValidPassword_ShouldReturnHash()
    {
        // Arrange
        const string password = "Senha@123";

        // Act
        var hash = _passwordHasher.Hash(password);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(hash));
    }

    [Fact]
    public void Hash_WithValidPassword_ShouldReturnValueDifferentFromPassword()
    {
        // Arrange
        const string password = "Senha@123";

        // Act
        var hash = _passwordHasher.Hash(password);

        // Assert
        Assert.NotEqual(password, hash);
    }

    [Fact]
    public void Hash_WithSamePasswordTwice_ShouldGenerateDifferentHashes()
    {
        // Arrange
        const string password = "Senha@123";

        // Act
        var firstHash =
            _passwordHasher.Hash(password);

        var secondHash =
            _passwordHasher.Hash(password);

        // Assert
        Assert.NotEqual(
            firstHash,
            secondHash);
    }

    [Fact]
    public void Hash_WithEmptyPassword_ShouldThrowArgumentException()
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => _passwordHasher.Hash(""));

        // Assert
        Assert.Equal(
            "password",
            exception.ParamName);

        Assert.Contains(
            "A senha é obrigatória.",
            exception.Message);
    }

    [Fact]
    public void Hash_WithWhiteSpacePassword_ShouldThrowArgumentException()
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => _passwordHasher.Hash("   "));

        // Assert
        Assert.Equal(
            "password",
            exception.ParamName);
    }

    [Fact]
    public void Verify_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        const string password = "Senha@123";

        var hash =
            _passwordHasher.Hash(password);

        // Act
        var result =
            _passwordHasher.Verify(
                password,
                hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Verify_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        const string password = "Senha@123";

        var hash =
            _passwordHasher.Hash(password);

        // Act
        var result =
            _passwordHasher.Verify(
                "SenhaErrada@123",
                hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Verify_WithEmptyPassword_ShouldReturnFalse()
    {
        // Arrange
        var hash =
            _passwordHasher.Hash(
                "Senha@123");

        // Act
        var result =
            _passwordHasher.Verify(
                "",
                hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Verify_WithWhiteSpacePassword_ShouldReturnFalse()
    {
        // Arrange
        var hash =
            _passwordHasher.Hash(
                "Senha@123");

        // Act
        var result =
            _passwordHasher.Verify(
                "   ",
                hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Verify_WithEmptyHash_ShouldReturnFalse()
    {
        // Act
        var result =
            _passwordHasher.Verify(
                "Senha@123",
                "");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Verify_WithWhiteSpaceHash_ShouldReturnFalse()
    {
        // Act
        var result =
            _passwordHasher.Verify(
                "Senha@123",
                "   ");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Verify_WithHashFromDifferentPassword_ShouldReturnFalse()
    {
        // Arrange
        var hash =
            _passwordHasher.Hash(
                "PrimeiraSenha@123");

        // Act
        var result =
            _passwordHasher.Verify(
                "SegundaSenha@123",
                hash);

        // Assert
        Assert.False(result);
    }
}