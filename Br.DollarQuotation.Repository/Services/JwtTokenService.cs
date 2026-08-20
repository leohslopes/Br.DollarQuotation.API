using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Br.DollarQuotation.Domain.Entities;
using Br.DollarQuotation.Domain.Interfaces.Services;
using Br.DollarQuotation.Repository.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Br.DollarQuotation.Repository.Services;

public sealed class JwtTokenService : ITokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;

        ValidateOptions();
    }

    public string GenerateAccessToken(User user, DateTime expiresAt)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (expiresAt <= DateTime.UtcNow)
        {
            throw new ArgumentException("A data de expiração do token deve ser futura.", nameof(expiresAt));
        }

        var claims = CreateClaims(user);
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var tokenDescriptor =
            new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _options.Issuer,
                Audience = _options.Audience,
                IssuedAt = DateTime.UtcNow,
                NotBefore = DateTime.UtcNow,
                Expires = expiresAt,
                SigningCredentials =signingCredentials
            };

        var tokenHandler = new JwtSecurityTokenHandler();

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    private static IEnumerable<Claim> CreateClaims(User user)
    {
        return
        [
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(JwtRegisteredClaimNames.Name, user.Name),
            new Claim(JwtRegisteredClaimNames.Email, user.Email.Value),
            new Claim(ClaimTypes.Email, user.Email.Value),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ];
    }

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(_options.SecretKey))
        {
            throw new InvalidOperationException("A chave secreta do JWT não foi configurada.");
        }

        if (_options.SecretKey.Length < 32)
        {
            throw new InvalidOperationException("A chave secreta do JWT deve possuir pelo menos 32 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(_options.Issuer))
        {
            throw new InvalidOperationException("O emissor do JWT não foi configurado.");
        }

        if (string.IsNullOrWhiteSpace(_options.Audience))
        {
            throw new InvalidOperationException("O público do JWT não foi configurado.");
        }
    }
}