using Br.DollarQuotation.Application.DTOs.Requests;
using Br.DollarQuotation.Application.Services;
using Br.DollarQuotation.Domain.Entities;
using Br.DollarQuotation.Domain.Exceptions;
using Br.DollarQuotation.Domain.Interfaces.Repositories;
using Br.DollarQuotation.Domain.Interfaces.Services;
using Br.DollarQuotation.Domain.ValueObjects;
using Br.DollarQuotation.Repository.Configurations;
using Microsoft.Extensions.Options;
using Moq;

namespace Br.DollarQuotation.Tests.Application;

public sealed class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordResetTokenRepository> _passwordResetTokenRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;

    private readonly JwtOptions _jwtOptions;
    private readonly PasswordResetOptions _passwordResetOptions;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordResetTokenRepositoryMock = new Mock<IPasswordResetTokenRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _tokenServiceMock = new Mock<ITokenService>();
        _emailServiceMock = new Mock<IEmailService>();

        _jwtOptions = new JwtOptions
        {
            SecretKey = "TEST_SECRET_KEY_123456789012345678901234567890",
            Issuer = "Br.DollarQuotation.Tests",
            Audience = "Br.DollarQuotation.Tests",
            ExpirationInMinutes = 60
        };

        _passwordResetOptions = new PasswordResetOptions
        {
            TokenExpirationInMinutes = 30,
            FrontendResetPasswordUrl = "http://localhost:4200/reset-password"
        };
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnLoginResponse()
    {
        // Arrange
        var user = CreateUser();

        var request = new LoginRequest
        {
            Email = "teste@email.com",
            Password = "Senha@123"
        };

        _userRepositoryMock
            .Setup(repository =>
                repository.GetByEmailAsync(
                    It.Is<Email>(
                        email =>
                            email.Value == "teste@email.com"),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(hasher =>
                hasher.Verify(
                    request.Password,
                    user.PasswordHash))
            .Returns(true);

        _tokenServiceMock
            .Setup(service =>
                service.GenerateAccessToken(
                    user,
                    It.IsAny<DateTime>()))
            .Returns("token-jwt-teste");

        var service = CreateService();

        // Act
        var response = await service.LoginAsync(request);

        // Assert
        Assert.NotNull(response);

        Assert.Equal(
            user.Id,
            response.UserId);

        Assert.Equal(
            user.Name,
            response.Name);

        Assert.Equal(
            user.Email.Value,
            response.Email);

        Assert.Equal(
            "token-jwt-teste",
            response.AccessToken);

        Assert.True(
            response.ExpiresAt > DateTime.UtcNow);

        _userRepositoryMock.Verify(
            repository =>
                repository.GetByEmailAsync(
                    It.IsAny<Email>(),
                    It.IsAny<CancellationToken>()),
            Times.Once);

        _passwordHasherMock.Verify(
            hasher =>
                hasher.Verify(
                    request.Password,
                    user.PasswordHash),
            Times.Once);

        _tokenServiceMock.Verify(
            tokenService =>
                tokenService.GenerateAccessToken(
                    user,
                    It.IsAny<DateTime>()),
            Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithNonExistingEmail_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "inexistente@email.com",
            Password = "Senha@123"
        };

        _userRepositoryMock
            .Setup(repository =>
                repository.GetByEmailAsync(
                    It.IsAny<Email>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var service = CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<InvalidCredentialsException>(
                () => service.LoginAsync(request));

        // Assert
        Assert.Equal(
            "E-mail ou senha inválidos.",
            exception.Message);

        _passwordHasherMock.Verify(
            hasher =>
                hasher.Verify(
                    It.IsAny<string>(),
                    It.IsAny<string>()),
            Times.Never);

        _tokenServiceMock.Verify(
            tokenService =>
                tokenService.GenerateAccessToken(
                    It.IsAny<User>(),
                    It.IsAny<DateTime>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var user = CreateUser();

        var request = new LoginRequest
        {
            Email = "teste@email.com",
            Password = "senha-incorreta"
        };

        _userRepositoryMock
            .Setup(repository =>
                repository.GetByEmailAsync(
                    It.IsAny<Email>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(hasher =>
                hasher.Verify(
                    request.Password,
                    user.PasswordHash))
            .Returns(false);

        var service = CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<InvalidCredentialsException>(
                () => service.LoginAsync(request));

        // Assert
        Assert.Equal(
            "E-mail ou senha inválidos.",
            exception.Message);

        _tokenServiceMock.Verify(
            tokenService =>
                tokenService.GenerateAccessToken(
                    It.IsAny<User>(),
                    It.IsAny<DateTime>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ShouldThrowInactiveUserException()
    {
        // Arrange
        var user = CreateUser();

        user.Deactivate();

        var request = new LoginRequest
        {
            Email = "teste@email.com",
            Password = "Senha@123"
        };

        _userRepositoryMock
            .Setup(repository =>
                repository.GetByEmailAsync(
                    It.IsAny<Email>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(hasher =>
                hasher.Verify(
                    request.Password,
                    user.PasswordHash))
            .Returns(true);

        var service = CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<InactiveUserException>(
                () => service.LoginAsync(request));

        // Assert
        Assert.Equal(
            "O usuário está inativo e não pode acessar o sistema.",
            exception.Message);

        _tokenServiceMock.Verify(
            tokenService =>
                tokenService.GenerateAccessToken(
                    It.IsAny<User>(),
                    It.IsAny<DateTime>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithEmptyEmail_ShouldThrowDomainException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "",
            Password = "Senha@123"
        };

        var service = CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () => service.LoginAsync(request));

        // Assert
        Assert.Equal(
            "O e-mail é obrigatório.",
            exception.Message);

        _userRepositoryMock.Verify(
            repository =>
                repository.GetByEmailAsync(
                    It.IsAny<Email>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithEmptyPassword_ShouldThrowDomainException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "teste@email.com",
            Password = ""
        };

        var service = CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () => service.LoginAsync(request));

        // Assert
        Assert.Equal(
            "A senha é obrigatória.",
            exception.Message);

        _userRepositoryMock.Verify(
            repository =>
                repository.GetByEmailAsync(
                    It.IsAny<Email>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithNullRequest_ShouldThrowDomainException()
    {
        // Arrange
        var service = CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () => service.LoginAsync(null!));

        // Assert
        Assert.Equal(
            "Os dados do login são obrigatórios.",
            exception.Message);
    }

    [Fact]
    public async Task LoginAsync_ShouldGenerateTokenWithConfiguredExpiration()
    {
        // Arrange
        var user = CreateUser();

        var request = new LoginRequest
        {
            Email = "teste@email.com",
            Password = "Senha@123"
        };

        DateTime? generatedExpiration = null;

        _userRepositoryMock
            .Setup(repository =>
                repository.GetByEmailAsync(
                    It.IsAny<Email>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(hasher =>
                hasher.Verify(
                    request.Password,
                    user.PasswordHash))
            .Returns(true);

        _tokenServiceMock
            .Setup(tokenService =>
                tokenService.GenerateAccessToken(
                    user,
                    It.IsAny<DateTime>()))
            .Callback<User, DateTime>(
                (_, expiresAt) =>
                    generatedExpiration = expiresAt)
            .Returns("token-jwt-teste");

        var service = CreateService();

        var beforeLogin = DateTime.UtcNow;

        // Act
        var response =
            await service.LoginAsync(request);

        var afterLogin = DateTime.UtcNow;

        // Assert
        Assert.NotNull(generatedExpiration);

        var minimumExpiration =
            beforeLogin.AddMinutes(
                _jwtOptions.ExpirationInMinutes);

        var maximumExpiration =
            afterLogin.AddMinutes(
                _jwtOptions.ExpirationInMinutes);

        Assert.InRange(
            generatedExpiration.Value,
            minimumExpiration,
            maximumExpiration);

        Assert.Equal(
            generatedExpiration.Value,
            response.ExpiresAt);
    }

    private AuthService CreateService()
    {
        return new AuthService(
            _userRepositoryMock.Object,
            _passwordResetTokenRepositoryMock.Object,
            _passwordHasherMock.Object,
            _tokenServiceMock.Object,
            _emailServiceMock.Object,
            Options.Create(_jwtOptions),
            Options.Create(_passwordResetOptions));
    }

    private static User CreateUser()
    {
        return new User(
            "Leonardo",
            Email.Create("teste@email.com"),
            "hash-da-senha");
    }
}