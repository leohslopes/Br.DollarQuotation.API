using System.Security.Cryptography;
using System.Text;
using Br.DollarQuotation.Application.DTOs.Requests;
using Br.DollarQuotation.Application.DTOs.Responses;
using Br.DollarQuotation.Application.Interfaces.Services;
using Br.DollarQuotation.Domain.Entities;
using Br.DollarQuotation.Domain.Exceptions;
using Br.DollarQuotation.Domain.Interfaces.Repositories;
using Br.DollarQuotation.Domain.Interfaces.Services;
using Br.DollarQuotation.Domain.ValueObjects;
using Br.DollarQuotation.Repository.Configurations;
using Microsoft.Extensions.Options;

namespace Br.DollarQuotation.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly JwtOptions _jwtOptions;
    private readonly PasswordResetOptions _passwordResetOptions;

    public AuthService(IUserRepository userRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IEmailService emailService,
        IOptions<JwtOptions> jwtOptions,
        IOptions<PasswordResetOptions> passwordResetOptions)
    {
        _userRepository = userRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _emailService = emailService;
        _jwtOptions = jwtOptions.Value;
        _passwordResetOptions = passwordResetOptions.Value;
    }


    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        ValidateLoginRequest(request);

        var email = Email.Create(request.Email);
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken) ?? throw new InvalidCredentialsException();
        var isValidPassword =_passwordHasher.Verify(request.Password, user.PasswordHash);

        if (!isValidPassword)
        {
            throw new InvalidCredentialsException();
        }

        if (!user.IsActive)
        {
            throw new InactiveUserException();
        }

        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationInMinutes);
        var accessToken =_tokenService.GenerateAccessToken(user, expiresAt);

        return new LoginResponse
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email.Value,
            PhotoBase64 = user.PhotoBase64,
            PhotoContentType = user.PhotoContentType,
            Role = user.Role.ToString(),
            AccessToken = accessToken,
            ExpiresAt = expiresAt
        };
    }


    public async Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        ValidateForgotPasswordRequest(request);

        var email = Email.Create(request.Email);
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return;
        }

        await _passwordResetTokenRepository.InvalidateActiveTokensByUserIdAsync(user.Id, cancellationToken);

        var rawToken = GenerateResetToken();
        var tokenHash = ComputeTokenHash(rawToken);
        var expiresAt = DateTime.UtcNow.AddMinutes(_passwordResetOptions.TokenExpirationInMinutes);
        var resetToken = new PasswordResetToken(user.Id, tokenHash, expiresAt);

        await _passwordResetTokenRepository.AddAsync(resetToken, cancellationToken);
        await _passwordResetTokenRepository.SaveChangesAsync(cancellationToken);

        var resetLink = BuildResetLink(rawToken);

        await _emailService.SendPasswordResetAsync(user.Email.Value, user.Name, resetLink, cancellationToken);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        ValidateResetPasswordRequest(request);

        var tokenHash = ComputeTokenHash(request.Token);

        var resetToken = await _passwordResetTokenRepository.GetValidByTokenHashAsync(tokenHash, cancellationToken) 
            ?? throw new DomainException( "O token de recuperação é inválido ou expirou.");

        var user = await _userRepository.GetByIdAsync(resetToken.UserId, cancellationToken) 
            ?? throw new DomainException("O usuário associado ao token não foi encontrado.");

        if (!user.IsActive)
        {
            throw new DomainException("O usuário associado ao token está inativo.");
        }

        var passwordHash =_passwordHasher.Hash(request.NewPassword);

        user.UpdatePassword(passwordHash);

        resetToken.MarkAsUsed();

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _passwordResetTokenRepository.SaveChangesAsync(cancellationToken);
    }

    private static void ValidateLoginRequest(LoginRequest request)
    {
        if (request is null)
        {
            throw new DomainException( "Os dados do login são obrigatórios.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new DomainException( "O e-mail é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new DomainException("A senha é obrigatória.");
        }
    }

    private static void ValidateForgotPasswordRequest(ForgotPasswordRequest request)
    {
        if (request is null)
        {
            throw new DomainException( "Os dados de recuperação de senha são obrigatórios.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new DomainException("O e-mail é obrigatório.");
        }
    }

    private static void ValidateResetPasswordRequest(ResetPasswordRequest request)
    {
        if (request is null)
        {
            throw new DomainException("Os dados para redefinição da senha são obrigatórios." );
        }

        if (string.IsNullOrWhiteSpace(request.Token))
        {
            throw new DomainException( "O token de recuperação é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword))
        {
            throw new DomainException("A nova senha é obrigatória.");
        }

        if (request.NewPassword.Length < 8)
        {
            throw new DomainException("A nova senha deve possuir no mínimo 8 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(request.ConfirmPassword))
        {
            throw new DomainException("A confirmação da nova senha é obrigatória.");
        }

        if (!string.Equals(request.NewPassword,request.ConfirmPassword,StringComparison.Ordinal))
        {
            throw new DomainException("A nova senha e a confirmação não conferem.");
        }
    }

    private static string GenerateResetToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToHexString(bytes);
    }

    private static string ComputeTokenHash(string token)
    {
        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var hashBytes = SHA256.HashData(tokenBytes);

        return Convert.ToHexString(hashBytes);
    }


    private string BuildResetLink(string rawToken)
    {
        if (string.IsNullOrWhiteSpace(_passwordResetOptions.FrontendResetPasswordUrl))
        {
            throw new InvalidOperationException("A URL de redefinição de senha do frontend não foi configurada.");
        }

        var separator =_passwordResetOptions.FrontendResetPasswordUrl.Contains('?') ? "&" : "?";

        return $"{_passwordResetOptions.FrontendResetPasswordUrl}" +
            $"{separator}token={Uri.EscapeDataString(rawToken)}";
    }
}