using Br.DollarQuotation.Domain.Entities;

namespace Br.DollarQuotation.Domain.Interfaces.Repositories;

public interface IPasswordResetTokenRepository
{
    Task AddAsync(PasswordResetToken resetToken, CancellationToken cancellationToken = default);

    Task<PasswordResetToken?> GetValidByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task InvalidateActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}