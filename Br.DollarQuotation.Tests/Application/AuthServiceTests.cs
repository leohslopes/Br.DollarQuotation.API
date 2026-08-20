using System.Security.Cryptography;
using System.Text;
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
using Br.DollarQuotation.Domain.Enums;

namespace Br.DollarQuotation.Tests.Application;

public sealed class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordResetTokenRepository>
        _passwordResetTokenRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;

    private readonly JwtOptions _jwtOptions;
    private readonly PasswordResetOptions _passwordResetOptions;

    public AuthServiceTests()
    {
        _userRepositoryMock =
            new Mock<IUserRepository>();

        _passwordResetTokenRepositoryMock =
            new Mock<IPasswordResetTokenRepository>();

        _passwordHasherMock =
            new Mock<IPasswordHasher>();

        _tokenServiceMock =
            new Mock<ITokenService>();

        _emailServiceMock =
            new Mock<IEmailService>();

        _jwtOptions =
            new JwtOptions
            {
                SecretKey =
                    "TEST_SECRET_KEY_123456789012345678901234567890",

                Issuer =
                    "Br.DollarQuotation.Tests",

                Audience =
                    "Br.DollarQuotation.Tests",

                ExpirationInMinutes =
                    60
            };

        _passwordResetOptions =
            new PasswordResetOptions
            {
                TokenExpirationInMinutes =
                    30,

                FrontendResetPasswordUrl =
                    "http://localhost:4200/reset-password"
            };
    }

    // =========================================================
    // LOGIN
    // =========================================================

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnLoginResponse()
    {
        // Arrange
        var user =
            CreateUser();

        var request =
            new LoginRequest
            {
                Email =
                    "teste@email.com",

                Password =
                    "Senha@123"
            };

        _userRepositoryMock
            .Setup(
                repository =>
                    repository.GetByEmailAsync(
                        It.Is<Email>(
                            email =>
                                email.Value ==
                                "teste@email.com"),
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                user);

        _passwordHasherMock
            .Setup(
                hasher =>
                    hasher.Verify(
                        request.Password,
                        user.PasswordHash))
            .Returns(
                true);

        _tokenServiceMock
            .Setup(
                service =>
                    service.GenerateAccessToken(
                        user,
                        It.IsAny<DateTime>()))
            .Returns(
                "token-jwt-teste");

        var service =
            CreateService();

        // Act
        var response =
            await service.LoginAsync(
                request);

        // Assert
        Assert.NotNull(
            response);

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
           user.Role.ToString(),
           response.Role);

        Assert.Equal(
            "token-jwt-teste",
            response.AccessToken);

        Assert.True(
            response.ExpiresAt >
            DateTime.UtcNow);

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
        var request =
            new LoginRequest
            {
                Email =
                    "inexistente@email.com",

                Password =
                    "Senha@123"
            };

        _userRepositoryMock
            .Setup(
                repository =>
                    repository.GetByEmailAsync(
                        It.IsAny<Email>(),
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (User?)null);

        var service =
            CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<
                InvalidCredentialsException>(
                () =>
                    service.LoginAsync(
                        request));

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
        var user =
            CreateUser();

        var request =
            new LoginRequest
            {
                Email =
                    "teste@email.com",

                Password =
                    "senha-incorreta"
            };

        _userRepositoryMock
            .Setup(
                repository =>
                    repository.GetByEmailAsync(
                        It.IsAny<Email>(),
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                user);

        _passwordHasherMock
            .Setup(
                hasher =>
                    hasher.Verify(
                        request.Password,
                        user.PasswordHash))
            .Returns(
                false);

        var service =
            CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<
                InvalidCredentialsException>(
                () =>
                    service.LoginAsync(
                        request));

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
        var user =
            CreateUser();

        user.Deactivate();

        var request =
            new LoginRequest
            {
                Email =
                    "teste@email.com",

                Password =
                    "Senha@123"
            };

        _userRepositoryMock
            .Setup(
                repository =>
                    repository.GetByEmailAsync(
                        It.IsAny<Email>(),
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                user);

        _passwordHasherMock
            .Setup(
                hasher =>
                    hasher.Verify(
                        request.Password,
                        user.PasswordHash))
            .Returns(
                true);

        var service =
            CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<
                InactiveUserException>(
                () =>
                    service.LoginAsync(
                        request));

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
        var request =
            new LoginRequest
            {
                Email = "",
                Password = "Senha@123"
            };

        var service =
            CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () =>
                    service.LoginAsync(
                        request));

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
        var request =
            new LoginRequest
            {
                Email =
                    "teste@email.com",

                Password = ""
            };

        var service =
            CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () =>
                    service.LoginAsync(
                        request));

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
        var service =
            CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () =>
                    service.LoginAsync(
                        null!));

        // Assert
        Assert.Equal(
            "Os dados do login são obrigatórios.",
            exception.Message);
    }

    [Fact]
    public async Task LoginAsync_ShouldGenerateTokenWithConfiguredExpiration()
    {
        // Arrange
        var user =
            CreateUser();

        var request =
            new LoginRequest
            {
                Email =
                    "teste@email.com",

                Password =
                    "Senha@123"
            };

        DateTime? generatedExpiration =
            null;

        _userRepositoryMock
            .Setup(
                repository =>
                    repository.GetByEmailAsync(
                        It.IsAny<Email>(),
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                user);

        _passwordHasherMock
            .Setup(
                hasher =>
                    hasher.Verify(
                        request.Password,
                        user.PasswordHash))
            .Returns(
                true);

        _tokenServiceMock
            .Setup(
                tokenService =>
                    tokenService.GenerateAccessToken(
                        user,
                        It.IsAny<DateTime>()))
            .Callback<User, DateTime>(
                (_, expiresAt) =>
                    generatedExpiration =
                        expiresAt)
            .Returns(
                "token-jwt-teste");

        var service =
            CreateService();

        var beforeLogin =
            DateTime.UtcNow;

        // Act
        var response =
            await service.LoginAsync(
                request);

        var afterLogin =
            DateTime.UtcNow;

        // Assert
        Assert.NotNull(
            generatedExpiration);

        var minimumExpiration =
            beforeLogin.AddMinutes(
                _jwtOptions
                    .ExpirationInMinutes);

        var maximumExpiration =
            afterLogin.AddMinutes(
                _jwtOptions
                    .ExpirationInMinutes);

        Assert.InRange(
            generatedExpiration.Value,
            minimumExpiration,
            maximumExpiration);

        Assert.Equal(
            generatedExpiration.Value,
            response.ExpiresAt);
    }

    // =========================================================
    // FORGOT PASSWORD
    // =========================================================

    [Fact]
    public async Task ForgotPasswordAsync_WithValidUser_ShouldCreateTokenAndSendEmail()
    {
        // Arrange
        var user =
            CreateUser();

        var request =
            new ForgotPasswordRequest
            {
                Email =
                    "teste@email.com"
            };

        PasswordResetToken? createdToken =
            null;

        string? resetLink =
            null;

        _userRepositoryMock
            .Setup(
                repository =>
                    repository.GetByEmailAsync(
                        It.Is<Email>(
                            email =>
                                email.Value ==
                                "teste@email.com"),
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                user);

        _passwordResetTokenRepositoryMock
            .Setup(
                repository =>
                    repository.AddAsync(
                        It.IsAny<PasswordResetToken>(),
                        It.IsAny<CancellationToken>()))
            .Callback<
                PasswordResetToken,
                CancellationToken>(
                (token, _) =>
                    createdToken =
                        token);

        _emailServiceMock
            .Setup(
                service =>
                    service.SendPasswordResetAsync(
                        user.Email.Value,
                        user.Name,
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()))
            .Callback<
                string,
                string,
                string,
                CancellationToken>(
                (_, _, link, _) =>
                    resetLink =
                        link);

        var service =
            CreateService();

        var before =
            DateTime.UtcNow;

        // Act
        await service.ForgotPasswordAsync(
            request);

        var after =
            DateTime.UtcNow;

        // Assert
        Assert.NotNull(
            createdToken);

        Assert.NotNull(
            resetLink);

        Assert.Equal(
            user.Id,
            createdToken.UserId);

        Assert.False(
            createdToken.IsUsed);

        Assert.True(
            createdToken.ExpiresAt >
            DateTime.UtcNow);

        Assert.InRange(
            createdToken.ExpiresAt,
            before.AddMinutes(
                _passwordResetOptions
                    .TokenExpirationInMinutes),
            after.AddMinutes(
                _passwordResetOptions
                    .TokenExpirationInMinutes));

        Assert.StartsWith(
            _passwordResetOptions
                .FrontendResetPasswordUrl +
            "?token=",
            resetLink);

        var rawToken =
            ExtractTokenFromResetLink(
                resetLink);

        Assert.False(
            string.IsNullOrWhiteSpace(
                rawToken));

        Assert.Equal(
            ComputeTokenHash(
                rawToken),
            createdToken.TokenHash);

        _passwordResetTokenRepositoryMock.Verify(
            repository =>
                repository
                    .InvalidateActiveTokensByUserIdAsync(
                        user.Id,
                        It.IsAny<CancellationToken>()),
            Times.Once);

        _passwordResetTokenRepositoryMock.Verify(
            repository =>
                repository.AddAsync(
                    It.IsAny<PasswordResetToken>(),
                    It.IsAny<CancellationToken>()),
            Times.Once);

        _passwordResetTokenRepositoryMock.Verify(
            repository =>
                repository.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
            Times.Once);

        _emailServiceMock.Verify(
            emailService =>
                emailService.SendPasswordResetAsync(
                    user.Email.Value,
                    user.Name,
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ForgotPasswordAsync_WithNonExistingUser_ShouldNotCreateTokenOrSendEmail()
    {
        // Arrange
        var request =
            new ForgotPasswordRequest
            {
                Email =
                    "inexistente@email.com"
            };

        _userRepositoryMock
            .Setup(
                repository =>
                    repository.GetByEmailAsync(
                        It.IsAny<Email>(),
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (User?)null);

        var service =
            CreateService();

        // Act
        await service.ForgotPasswordAsync(
            request);

        // Assert
        _passwordResetTokenRepositoryMock.Verify(
            repository =>
                repository
                    .InvalidateActiveTokensByUserIdAsync(
                        It.IsAny<Guid>(),
                        It.IsAny<CancellationToken>()),
            Times.Never);

        _passwordResetTokenRepositoryMock.Verify(
            repository =>
                repository.AddAsync(
                    It.IsAny<PasswordResetToken>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);

        _passwordResetTokenRepositoryMock.Verify(
            repository =>
                repository.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
            Times.Never);

        _emailServiceMock.Verify(
            service =>
                service.SendPasswordResetAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ForgotPasswordAsync_WithInactiveUser_ShouldNotCreateTokenOrSendEmail()
    {
        // Arrange
        var user =
            CreateUser();

        user.Deactivate();

        var request =
            new ForgotPasswordRequest
            {
                Email =
                    user.Email.Value
            };

        _userRepositoryMock
            .Setup(
                repository =>
                    repository.GetByEmailAsync(
                        It.IsAny<Email>(),
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                user);

        var service =
            CreateService();

        // Act
        await service.ForgotPasswordAsync(
            request);

        // Assert
        _passwordResetTokenRepositoryMock.Verify(
            repository =>
                repository
                    .InvalidateActiveTokensByUserIdAsync(
                        It.IsAny<Guid>(),
                        It.IsAny<CancellationToken>()),
            Times.Never);

        _passwordResetTokenRepositoryMock.Verify(
            repository =>
                repository.AddAsync(
                    It.IsAny<PasswordResetToken>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);

        _emailServiceMock.Verify(
            service =>
                service.SendPasswordResetAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ForgotPasswordAsync_WithEmptyEmail_ShouldThrowDomainException()
    {
        // Arrange
        var request =
            new ForgotPasswordRequest
            {
                Email = ""
            };

        var service =
            CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () =>
                    service.ForgotPasswordAsync(
                        request));

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
    public async Task ForgotPasswordAsync_WithNullRequest_ShouldThrowDomainException()
    {
        // Arrange
        var service =
            CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () =>
                    service.ForgotPasswordAsync(
                        null!));

        // Assert
        Assert.Equal(
            "Os dados de recuperação de senha são obrigatórios.",
            exception.Message);
    }

    [Fact]
    public async Task ForgotPasswordAsync_ShouldInvalidatePreviousTokensBeforeCreatingNewToken()
    {
        // Arrange
        var user =
            CreateUser();

        var request =
            new ForgotPasswordRequest
            {
                Email =
                    user.Email.Value
            };

        var sequence =
            new MockSequence();

        _userRepositoryMock
            .Setup(
                repository =>
                    repository.GetByEmailAsync(
                        It.IsAny<Email>(),
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                user);

        _passwordResetTokenRepositoryMock
            .InSequence(sequence)
            .Setup(
                repository =>
                    repository
                        .InvalidateActiveTokensByUserIdAsync(
                            user.Id,
                            It.IsAny<CancellationToken>()));

        _passwordResetTokenRepositoryMock
            .InSequence(sequence)
            .Setup(
                repository =>
                    repository.AddAsync(
                        It.IsAny<PasswordResetToken>(),
                        It.IsAny<CancellationToken>()));

        var service =
            CreateService();

        // Act
        await service.ForgotPasswordAsync(
            request);

        // Assert
        _passwordResetTokenRepositoryMock.Verify(
            repository =>
                repository
                    .InvalidateActiveTokensByUserIdAsync(
                        user.Id,
                        It.IsAny<CancellationToken>()),
            Times.Once);

        _passwordResetTokenRepositoryMock.Verify(
            repository =>
                repository.AddAsync(
                    It.IsAny<PasswordResetToken>(),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // =========================================================
    // RESET PASSWORD
    // =========================================================

    [Fact]
    public async Task ResetPasswordAsync_WithValidToken_ShouldUpdatePasswordAndMarkTokenAsUsed()
    {
        // Arrange
        var user =
            CreateUser();

        const string rawToken =
            "TOKEN-DE-RECUPERACAO-VALIDO";

        var tokenHash =
            ComputeTokenHash(
                rawToken);

        var resetToken =
            new PasswordResetToken(
                user.Id,
                tokenHash,
                DateTime.UtcNow
                    .AddMinutes(30));

        var request =
            new ResetPasswordRequest
            {
                Token =
                    rawToken,

                NewPassword =
                    "NovaSenha@123",

                ConfirmPassword =
                    "NovaSenha@123"
            };

        _passwordResetTokenRepositoryMock
            .Setup(
                repository =>
                    repository
                        .GetValidByTokenHashAsync(
                            tokenHash,
                            It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                resetToken);

        _userRepositoryMock
            .Setup(
                repository =>
                    repository.GetByIdAsync(
                        user.Id,
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                user);

        _passwordHasherMock
            .Setup(
                hasher =>
                    hasher.Hash(
                        request.NewPassword))
            .Returns(
                "novo-hash-da-senha");

        var service =
            CreateService();

        // Act
        await service.ResetPasswordAsync(
            request);

        // Assert
        Assert.Equal(
            "novo-hash-da-senha",
            user.PasswordHash);

        Assert.True(
            resetToken.IsUsed);

        Assert.NotNull(
            resetToken.UsedAt);

        _passwordHasherMock.Verify(
            hasher =>
                hasher.Hash(
                    request.NewPassword),
            Times.Once);

        _userRepositoryMock.Verify(
            repository =>
                repository.UpdateAsync(
                    user,
                    It.IsAny<CancellationToken>()),
            Times.Once);

        _passwordResetTokenRepositoryMock.Verify(
            repository =>
                repository.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithInvalidToken_ShouldThrowDomainException()
    {
        // Arrange
        const string rawToken =
            "TOKEN-INVALIDO";

        var tokenHash =
            ComputeTokenHash(
                rawToken);

        var request =
            new ResetPasswordRequest
            {
                Token =
                    rawToken,

                NewPassword =
                    "NovaSenha@123",

                ConfirmPassword =
                    "NovaSenha@123"
            };

        _passwordResetTokenRepositoryMock
            .Setup(
                repository =>
                    repository
                        .GetValidByTokenHashAsync(
                            tokenHash,
                            It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (PasswordResetToken?)null);

        var service =
            CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () =>
                    service.ResetPasswordAsync(
                        request));

        // Assert
        Assert.Equal(
            "O token de recuperação é inválido ou expirou.",
            exception.Message);

        _userRepositoryMock.Verify(
            repository =>
                repository.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);

        _passwordHasherMock.Verify(
            hasher =>
                hasher.Hash(
                    It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task ResetPasswordAsync_WhenUserDoesNotExist_ShouldThrowDomainException()
    {
        // Arrange
        var user =
            CreateUser();

        const string rawToken =
            "TOKEN-USUARIO-INEXISTENTE";

        var tokenHash =
            ComputeTokenHash(
                rawToken);

        var resetToken =
            new PasswordResetToken(
                user.Id,
                tokenHash,
                DateTime.UtcNow
                    .AddMinutes(30));

        var request =
            new ResetPasswordRequest
            {
                Token =
                    rawToken,

                NewPassword =
                    "NovaSenha@123",

                ConfirmPassword =
                    "NovaSenha@123"
            };

        _passwordResetTokenRepositoryMock
            .Setup(
                repository =>
                    repository
                        .GetValidByTokenHashAsync(
                            tokenHash,
                            It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                resetToken);

        _userRepositoryMock
            .Setup(
                repository =>
                    repository.GetByIdAsync(
                        resetToken.UserId,
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (User?)null);

        var service =
            CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () =>
                    service.ResetPasswordAsync(
                        request));

        // Assert
        Assert.Equal(
            "O usuário associado ao token não foi encontrado.",
            exception.Message);

        _passwordHasherMock.Verify(
            hasher =>
                hasher.Hash(
                    It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithInactiveUser_ShouldThrowDomainException()
    {
        // Arrange
        var user =
            CreateUser();

        user.Deactivate();

        const string rawToken =
            "TOKEN-USUARIO-INATIVO";

        var tokenHash =
            ComputeTokenHash(
                rawToken);

        var resetToken =
            new PasswordResetToken(
                user.Id,
                tokenHash,
                DateTime.UtcNow
                    .AddMinutes(30));

        var request =
            new ResetPasswordRequest
            {
                Token =
                    rawToken,

                NewPassword =
                    "NovaSenha@123",

                ConfirmPassword =
                    "NovaSenha@123"
            };

        _passwordResetTokenRepositoryMock
            .Setup(
                repository =>
                    repository
                        .GetValidByTokenHashAsync(
                            tokenHash,
                            It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                resetToken);

        _userRepositoryMock
            .Setup(
                repository =>
                    repository.GetByIdAsync(
                        user.Id,
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                user);

        var service =
            CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () =>
                    service.ResetPasswordAsync(
                        request));

        // Assert
        Assert.Equal(
            "O usuário associado ao token está inativo.",
            exception.Message);

        _passwordHasherMock.Verify(
            hasher =>
                hasher.Hash(
                    It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithDifferentPasswords_ShouldThrowDomainException()
    {
        // Arrange
        var request =
            new ResetPasswordRequest
            {
                Token =
                    "TOKEN",

                NewPassword =
                    "NovaSenha@123",

                ConfirmPassword =
                    "OutraSenha@123"
            };

        var service =
            CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () =>
                    service.ResetPasswordAsync(
                        request));

        // Assert
        Assert.Equal(
            "A nova senha e a confirmação não conferem.",
            exception.Message);

        _passwordResetTokenRepositoryMock.Verify(
            repository =>
                repository
                    .GetValidByTokenHashAsync(
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithPasswordLessThanEightCharacters_ShouldThrowDomainException()
    {
        // Arrange
        var request =
            new ResetPasswordRequest
            {
                Token =
                    "TOKEN",

                NewPassword =
                    "1234567",

                ConfirmPassword =
                    "1234567"
            };

        var service =
            CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () =>
                    service.ResetPasswordAsync(
                        request));

        // Assert
        Assert.Equal(
            "A nova senha deve possuir no mínimo 8 caracteres.",
            exception.Message);

        _passwordResetTokenRepositoryMock.Verify(
            repository =>
                repository
                    .GetValidByTokenHashAsync(
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithEmptyToken_ShouldThrowDomainException()
    {
        // Arrange
        var request =
            new ResetPasswordRequest
            {
                Token = "",

                NewPassword =
                    "NovaSenha@123",

                ConfirmPassword =
                    "NovaSenha@123"
            };

        var service =
            CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () =>
                    service.ResetPasswordAsync(
                        request));

        // Assert
        Assert.Equal(
            "O token de recuperação é obrigatório.",
            exception.Message);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithEmptyNewPassword_ShouldThrowDomainException()
    {
        // Arrange
        var request =
            new ResetPasswordRequest
            {
                Token =
                    "TOKEN",

                NewPassword = "",

                ConfirmPassword = ""
            };

        var service =
            CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () =>
                    service.ResetPasswordAsync(
                        request));

        // Assert
        Assert.Equal(
            "A nova senha é obrigatória.",
            exception.Message);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithEmptyConfirmation_ShouldThrowDomainException()
    {
        // Arrange
        var request =
            new ResetPasswordRequest
            {
                Token =
                    "TOKEN",

                NewPassword =
                    "NovaSenha@123",

                ConfirmPassword = ""
            };

        var service =
            CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () =>
                    service.ResetPasswordAsync(
                        request));

        // Assert
        Assert.Equal(
            "A confirmação da nova senha é obrigatória.",
            exception.Message);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithNullRequest_ShouldThrowDomainException()
    {
        // Arrange
        var service =
            CreateService();

        // Act
        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () =>
                    service.ResetPasswordAsync(
                        null!));

        // Assert
        Assert.Equal(
            "Os dados para redefinição da senha são obrigatórios.",
            exception.Message);
    }

    [Fact]
    public async Task ResetPasswordAsync_ShouldSearchTokenUsingSha256Hash()
    {
        // Arrange
        var user =
            CreateUser();

        const string rawToken =
            "TOKEN-PARA-VALIDAR-HASH";

        var expectedHash =
            ComputeTokenHash(
                rawToken);

        var resetToken =
            new PasswordResetToken(
                user.Id,
                expectedHash,
                DateTime.UtcNow
                    .AddMinutes(30));

        var request =
            new ResetPasswordRequest
            {
                Token =
                    rawToken,

                NewPassword =
                    "NovaSenha@123",

                ConfirmPassword =
                    "NovaSenha@123"
            };

        _passwordResetTokenRepositoryMock
            .Setup(
                repository =>
                    repository
                        .GetValidByTokenHashAsync(
                            expectedHash,
                            It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                resetToken);

        _userRepositoryMock
            .Setup(
                repository =>
                    repository.GetByIdAsync(
                        user.Id,
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                user);

        _passwordHasherMock
            .Setup(
                hasher =>
                    hasher.Hash(
                        request.NewPassword))
            .Returns(
                "novo-hash");

        var service =
            CreateService();

        // Act
        await service.ResetPasswordAsync(
            request);

        // Assert
        _passwordResetTokenRepositoryMock.Verify(
            repository =>
                repository
                    .GetValidByTokenHashAsync(
                        expectedHash,
                        It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithAdminUser_ShouldReturnAdminRole()
    {
        // Arrange
        var user =
            new User(
                "Administrador",
                Email.Create(
                    "admin@email.com"),
                "hash-da-senha",
                role: UserRole.Admin);

        var request =
            new LoginRequest
            {
                Email =
                    "admin@email.com",

                Password =
                    "Senha@123"
            };

        _userRepositoryMock
            .Setup(
                repository =>
                    repository.GetByEmailAsync(
                        It.Is<Email>(
                            email =>
                                email.Value ==
                                "admin@email.com"),
                        It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                user);

        _passwordHasherMock
            .Setup(
                hasher =>
                    hasher.Verify(
                        request.Password,
                        user.PasswordHash))
            .Returns(
                true);

        _tokenServiceMock
            .Setup(
                tokenService =>
                    tokenService.GenerateAccessToken(
                        user,
                        It.IsAny<DateTime>()))
            .Returns(
                "token-admin");

        var service =
            CreateService();

        // Act
        var response =
            await service.LoginAsync(
                request);

        // Assert
        Assert.NotNull(
            response);

        Assert.Equal(
            user.Id,
            response.UserId);

        Assert.Equal(
            "Administrador",
            response.Name);

        Assert.Equal(
            "admin@email.com",
            response.Email);

        Assert.Equal(
            UserRole.Admin.ToString(),
            response.Role);

        Assert.Equal(
            "Admin",
            response.Role);

        Assert.Equal(
            "token-admin",
            response.AccessToken);

        Assert.True(
            response.ExpiresAt >
            DateTime.UtcNow);

        _tokenServiceMock.Verify(
            tokenService =>
                tokenService.GenerateAccessToken(
                    user,
                    It.IsAny<DateTime>()),
            Times.Once);
    }

    // =========================================================
    // HELPERS
    // =========================================================

    private AuthService CreateService()
    {
        return new AuthService(
            _userRepositoryMock.Object,
            _passwordResetTokenRepositoryMock.Object,
            _passwordHasherMock.Object,
            _tokenServiceMock.Object,
            _emailServiceMock.Object,
            Options.Create(
                _jwtOptions),
            Options.Create(
                _passwordResetOptions));
    }

    private static User CreateUser()
    {
        return new User(
            "Leonardo",
            Email.Create(
                "teste@email.com"),
            "hash-da-senha");
    }

    private static string ComputeTokenHash(
        string token)
    {
        var tokenBytes =
            Encoding.UTF8.GetBytes(
                token);

        var hashBytes =
            SHA256.HashData(
                tokenBytes);

        return Convert.ToHexString(
            hashBytes);
    }

    private static string ExtractTokenFromResetLink(
        string resetLink)
    {
        var uri =
            new Uri(
                resetLink);

        var query =
            uri.Query
                .TrimStart('?');

        var queryParameters =
            query.Split(
                '&',
                StringSplitOptions.RemoveEmptyEntries);

        foreach (var parameter in queryParameters)
        {
            var parts =
                parameter.Split(
                    '=',
                    2);

            if (
                parts.Length == 2 &&
                string.Equals(
                    parts[0],
                    "token",
                    StringComparison.OrdinalIgnoreCase))
            {
                return Uri.UnescapeDataString(
                    parts[1]);
            }
        }

        return string.Empty;
    }
}