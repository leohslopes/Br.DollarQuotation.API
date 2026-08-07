using Br.DollarQuotation.Application.DTOs.Requests;
using Br.DollarQuotation.Application.DTOs.Responses;
using Br.DollarQuotation.Application.Interfaces.Services;
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
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly JwtOptions _jwtOptions;

    public AuthService(IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IOptions<JwtOptions> jwtOptions)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var email = Email.Create(request.Email);
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken) ?? throw new InvalidCredentialsException();
        var isValidPassword = _passwordHasher.Verify(request.Password, user.PasswordHash);

        if (!isValidPassword)
        {
            throw new InvalidCredentialsException();
        }

        if (!user.IsActive)
        {
            throw new InactiveUserException();
        }

        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationInMinutes);
        var accessToken = _tokenService.GenerateAccessToken(user, expiresAt);

        return new LoginResponse
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email.Value,
            PhotoBase64 = user.PhotoBase64,
            PhotoContentType = user.PhotoContentType,
            AccessToken = accessToken,
            ExpiresAt = expiresAt
        };
    }

    private static void ValidateRequest(LoginRequest request)
    {
        if (request is null)
        {
            throw new DomainException("Os dados do login são obrigatórios.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new DomainException("O e-mail é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new DomainException("A senha é obrigatória.");
        }
    }
}