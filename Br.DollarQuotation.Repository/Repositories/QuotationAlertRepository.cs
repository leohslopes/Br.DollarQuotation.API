using Br.DollarQuotation.Domain.Entities;
using Br.DollarQuotation.Domain.Interfaces.Repositories;
using Br.DollarQuotation.Domain.ValueObjects;
using Br.DollarQuotation.Repository.Context;
using Microsoft.EntityFrameworkCore;

namespace Br.DollarQuotation.Repository.Repositories;

public sealed class QuotationAlertRepository : IQuotationAlertRepository
{
    private readonly AppDbContext _context;

    public QuotationAlertRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        QuotationAlert alert,
        CancellationToken cancellationToken = default)
    {
        await _context.QuotationAlerts.AddAsync(
            alert,
            cancellationToken
        );
    }

    public async Task<QuotationAlert?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.QuotationAlerts
            .FirstOrDefaultAsync(
                alert => alert.Id == id,
                cancellationToken
            );
    }

    public async Task<IReadOnlyCollection<QuotationAlert>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.QuotationAlerts
            .AsNoTracking()
            .Where(
                alert => alert.UserId == userId
            )
            .OrderByDescending(
                alert => alert.CreatedAt
            )
            .ToListAsync(
                cancellationToken
            );
    }

    public async Task<IReadOnlyCollection<QuotationAlert>> GetActiveByCurrencyPairAsync(
        string currencyPair,
        CancellationToken cancellationToken = default)
    {
        var pair = CurrencyPair.FromCode(
            currencyPair
        );

        return await _context.QuotationAlerts
            .Where(
                alert =>
                    alert.CurrencyPair == pair &&
                    alert.IsActive
            )
            .OrderBy(
                alert => alert.CreatedAt
            )
            .ToListAsync(
                cancellationToken
            );
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(
            cancellationToken
        );
    }
}
