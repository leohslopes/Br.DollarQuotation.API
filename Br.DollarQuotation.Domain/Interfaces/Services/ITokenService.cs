using Br.DollarQuotation.Domain.Entities;

namespace Br.DollarQuotation.Domain.Interfaces.Services;

public interface ITokenService
{
    string GenerateAccessToken(User user, DateTime expiresAt);
}