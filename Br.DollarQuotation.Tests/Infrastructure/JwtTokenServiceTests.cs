using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Br.DollarQuotation.Domain.Entities;
using Br.DollarQuotation.Domain.ValueObjects;
using Br.DollarQuotation.Repository.Configurations;
using Br.DollarQuotation.Repository.Services;
using Microsoft.Extensions.Options;

namespace Br.DollarQuotation.Tests.Infrastructure;

public sealed class JwtTokenServiceTests
{
    private const string SecretKey =
        "BrDollarQuotation@TestJwtKey#2026!123456789";

    private const string Issuer =
        "Br.DollarQuotation.Tests";

    private const string Audience =
        "Br.DollarQuotation.Tests.Client";

    #region GenerateAccessToken

    [Fact]
    public void GenerateAccessToken_WithValidUser_ShouldReturnToken()
    {
        // Arrange
        var service = CreateService();
        var user = CreateUser();

        var expiresAt =
            DateTime.UtcNow.AddMinutes(60);

        // Act
        var token =
            service.GenerateAccessToken(
                user,
                expiresAt);

        // Assert
        Assert.False(
            string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public void GenerateAccessToken_ShouldGenerateValidJwtFormat()
    {
        // Arrange
        var service = CreateService();
        var user = CreateUser();

        var expiresAt =
            DateTime.UtcNow.AddMinutes(60);

        // Act
        var token =
            service.GenerateAccessToken(
                user,
                expiresAt);

        var handler =
            new JwtSecurityTokenHandler();

        // Assert
        Assert.True(
            handler.CanReadToken(token));

        var jwtToken =
            handler.ReadJwtToken(token);

        Assert.NotNull(jwtToken);
    }

    [Fact]
    public void GenerateAccessToken_ShouldContainCorrectIssuer()
    {
        // Arrange
        var service = CreateService();
        var user = CreateUser();

        // Act
        var token =
            service.GenerateAccessToken(
                user,
                DateTime.UtcNow.AddMinutes(60));

        var jwtToken =
            ReadToken(token);

        // Assert
        Assert.Equal(
            Issuer,
            jwtToken.Issuer);
    }

    [Fact]
    public void GenerateAccessToken_ShouldContainCorrectAudience()
    {
        // Arrange
        var service = CreateService();
        var user = CreateUser();

        // Act
        var token =
            service.GenerateAccessToken(
                user,
                DateTime.UtcNow.AddMinutes(60));

        var jwtToken =
            ReadToken(token);

        // Assert
        Assert.Contains(
            Audience,
            jwtToken.Audiences);
    }

    [Fact]
    public void GenerateAccessToken_ShouldContainUserIdInSubject()
    {
        // Arrange
        var service = CreateService();
        var user = CreateUser();

        // Act
        var token =
            service.GenerateAccessToken(
                user,
                DateTime.UtcNow.AddMinutes(60));

        var jwtToken =
            ReadToken(token);

        // Assert
        var subject =
            jwtToken.Claims.FirstOrDefault(
                claim =>
                    claim.Type ==
                    JwtRegisteredClaimNames.Sub);

        Assert.NotNull(subject);

        Assert.Equal(
            user.Id.ToString(),
            subject.Value);
    }

    [Fact]
    public void GenerateAccessToken_ShouldContainUserName()
    {
        // Arrange
        var service = CreateService();
        var user = CreateUser();

        // Act
        var token =
            service.GenerateAccessToken(
                user,
                DateTime.UtcNow.AddMinutes(60));

        var jwtToken =
            ReadToken(token);

        // Assert
        Assert.Contains(
            jwtToken.Claims,
            claim =>
                claim.Value == user.Name &&
                (
                    claim.Type ==
                        JwtRegisteredClaimNames.Name ||
                    claim.Type ==
                        ClaimTypes.Name
                ));
    }

    [Fact]
    public void GenerateAccessToken_ShouldContainUserEmail()
    {
        // Arrange
        var service = CreateService();
        var user = CreateUser();

        // Act
        var token =
            service.GenerateAccessToken(
                user,
                DateTime.UtcNow.AddMinutes(60));

        var jwtToken =
            ReadToken(token);

        // Assert
        Assert.Contains(
            jwtToken.Claims,
            claim =>
                claim.Value ==
                user.Email.Value);
    }

    [Fact]
    public void GenerateAccessToken_ShouldContainJti()
    {
        // Arrange
        var service = CreateService();
        var user = CreateUser();

        // Act
        var token =
            service.GenerateAccessToken(
                user,
                DateTime.UtcNow.AddMinutes(60));

        var jwtToken =
            ReadToken(token);

        // Assert
        var jti =
            jwtToken.Claims.FirstOrDefault(
                claim =>
                    claim.Type ==
                    JwtRegisteredClaimNames.Jti);

        Assert.NotNull(jti);

        Assert.True(
            Guid.TryParse(
                jti.Value,
                out _));
    }

    [Fact]
    public void GenerateAccessToken_CalledTwice_ShouldGenerateDifferentTokens()
    {
        // Arrange
        var service = CreateService();
        var user = CreateUser();

        var expiresAt =
            DateTime.UtcNow.AddMinutes(60);

        // Act
        var firstToken =
            service.GenerateAccessToken(
                user,
                expiresAt);

        var secondToken =
            service.GenerateAccessToken(
                user,
                expiresAt);

        // Assert
        Assert.NotEqual(
            firstToken,
            secondToken);
    }

    [Fact]
    public void GenerateAccessToken_ShouldUseProvidedExpiration()
    {
        // Arrange
        var service = CreateService();
        var user = CreateUser();

        var expiresAt =
            DateTime.UtcNow.AddMinutes(30);

        // Act
        var token =
            service.GenerateAccessToken(
                user,
                expiresAt);

        var jwtToken =
            ReadToken(token);

        // Assert
        Assert.Equal(
            expiresAt,
            jwtToken.ValidTo,
            TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void GenerateAccessToken_WithNullUser_ShouldThrowArgumentNullException()
    {
        // Arrange
        var service = CreateService();

        // Act
        var exception =
            Assert.Throws<ArgumentNullException>(
                () =>
                    service.GenerateAccessToken(
                        null!,
                        DateTime.UtcNow.AddMinutes(60)));

        // Assert
        Assert.Equal(
            "user",
            exception.ParamName);
    }

    [Fact]
    public void GenerateAccessToken_WithPastExpiration_ShouldThrowArgumentException()
    {
        // Arrange
        var service = CreateService();
        var user = CreateUser();

        var expiresAt =
            DateTime.UtcNow.AddMinutes(-1);

        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    service.GenerateAccessToken(
                        user,
                        expiresAt));

        // Assert
        Assert.Equal(
            "expiresAt",
            exception.ParamName);

        Assert.Contains(
            "A data de expiração do token deve ser futura.",
            exception.Message);
    }

    #endregion

    #region Configuration

    [Fact]
    public void Constructor_WithEmptySecretKey_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var options = CreateOptions();

        options.SecretKey = "";

        // Act
        var exception =
            Assert.Throws<InvalidOperationException>(
                () => CreateService(options));

        // Assert
        Assert.Equal(
            "A chave secreta do JWT não foi configurada.",
            exception.Message);
    }

    [Fact]
    public void Constructor_WithSecretKeyLessThan32Characters_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var options = CreateOptions();

        options.SecretKey =
            "chave-curta";

        // Act
        var exception =
            Assert.Throws<InvalidOperationException>(
                () => CreateService(options));

        // Assert
        Assert.Equal(
            "A chave secreta do JWT deve possuir pelo menos 32 caracteres.",
            exception.Message);
    }

    [Fact]
    public void Constructor_WithEmptyIssuer_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var options = CreateOptions();

        options.Issuer = "";

        // Act
        var exception =
            Assert.Throws<InvalidOperationException>(
                () => CreateService(options));

        // Assert
        Assert.Equal(
            "O emissor do JWT não foi configurado.",
            exception.Message);
    }

    [Fact]
    public void Constructor_WithEmptyAudience_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var options = CreateOptions();

        options.Audience = "";

        // Act
        var exception =
            Assert.Throws<InvalidOperationException>(
                () => CreateService(options));

        // Assert
        Assert.Equal(
            "O público do JWT não foi configurado.",
            exception.Message);
    }

    #endregion

    #region Helpers

    private static JwtTokenService CreateService()
    {
        return CreateService(
            CreateOptions());
    }

    private static JwtTokenService CreateService(
        JwtOptions jwtOptions)
    {
        return new JwtTokenService(
            Options.Create(jwtOptions));
    }

    private static JwtOptions CreateOptions()
    {
        return new JwtOptions
        {
            SecretKey = SecretKey,
            Issuer = Issuer,
            Audience = Audience,
            ExpirationInMinutes = 60
        };
    }

    private static User CreateUser()
    {
        return new User(
            "Leonardo",
            Email.Create(
                "teste@email.com"),
            "password-hash");
    }

    private static JwtSecurityToken ReadToken(
        string token)
    {
        var handler =
            new JwtSecurityTokenHandler();

        return handler.ReadJwtToken(token);
    }

    #endregion
}