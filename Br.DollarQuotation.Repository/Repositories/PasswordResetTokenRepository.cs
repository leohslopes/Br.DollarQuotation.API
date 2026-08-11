using Br.DollarQuotation.Domain.Entities;
using Br.DollarQuotation.Domain.Interfaces.Repositories;
using Br.DollarQuotation.Repository.Context;
using Microsoft.EntityFrameworkCore;

namespace Br.DollarQuotation.Repository.Repositories;

public sealed class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly AppDbContext _context;

    public PasswordResetTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(PasswordResetToken resetToken, CancellationToken cancellationToken = default)
    {
        await _context.PasswordResetTokens.AddAsync(resetToken, cancellationToken);
    }

    public async Task<PasswordResetToken?> GetValidByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await _context.PasswordResetTokens
            .FirstOrDefaultAsync(
                token =>
                    token.TokenHash == tokenHash &&
                    token.UsedAt == null &&
                    token.ExpiresAt > now,
                cancellationToken
            );
    }

    public async Task InvalidateActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var activeTokens =
            await _context.PasswordResetTokens
                .Where(
                    token =>
                        token.UserId == userId &&
                        token.UsedAt == null &&
                        token.ExpiresAt > now
                )
                .ToListAsync(
                    cancellationToken
                );

        foreach (var token in activeTokens)
        {
            token.MarkAsUsed();
        }
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}