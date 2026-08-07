using Br.DollarQuotation.Domain.Entities;
using Br.DollarQuotation.Domain.Interfaces.Repositories;
using Br.DollarQuotation.Domain.Models;
using Br.DollarQuotation.Domain.ValueObjects;
using Br.DollarQuotation.Repository.Context;
using Microsoft.EntityFrameworkCore;

namespace Br.DollarQuotation.Repository.Repositories;

public sealed class CurrencyQuotationRepository : ICurrencyQuotationRepository
{
    private readonly AppDbContext _context;

    public CurrencyQuotationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CurrencyQuotation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.CurrencyQuotations.AsNoTracking()
                     .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<CurrencyQuotation?> GetLatestAsync(CurrencyPair currencyPair, CancellationToken cancellationToken = default)
    {
        return await _context.CurrencyQuotations.AsNoTracking()
                     .Where(x => x.CurrencyPair == currencyPair)
                     .OrderByDescending(x => x.QuotationDate)
                     .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<CurrencyQuotation>> GetHistoryAsync(CurrencyPair currencyPair, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.CurrencyQuotations.AsNoTracking()
                     .Where(x => x.CurrencyPair == currencyPair && x.QuotationDate >= startDate && x.QuotationDate <= endDate)
                     .OrderBy(x => x.QuotationDate)
                     .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(CurrencyQuotation quotation, CancellationToken cancellationToken = default)
    {
        await _context.CurrencyQuotations.AddAsync(quotation, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<CurrencyQuotation> quotations, CancellationToken cancellationToken = default)
    {
        await _context.CurrencyQuotations.AddRangeAsync(quotations, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(CurrencyPair currencyPair, DateTime quotationDate, CancellationToken cancellationToken = default)
    {
        return await _context.CurrencyQuotations
                    .AsNoTracking()
                    .AnyAsync(x =>x.CurrencyPair == currencyPair && x.QuotationDate == quotationDate, cancellationToken);
    }

    public async Task<IReadOnlyCollection<CurrencyQuotation>> GetPagedAsync(CurrencyPair? currencyPair, DateTime? startDate, DateTime? endDate, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = BuildFilterQuery(currencyPair, startDate, endDate);

        return await query
            .AsNoTracking()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync( CurrencyPair? currencyPair, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
    {
        var query = BuildFilterQuery(
            currencyPair,
            startDate,
            endDate);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<CurrencyQuotationSummary?> GetSummaryAsync(CurrencyPair currencyPair, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
    {
        var query = BuildFilterQuery(currencyPair, startDate, endDate);

        if (!await query.AnyAsync(cancellationToken))
            return null;

        return await query
            .GroupBy(_ => 1)
            .Select(group => new CurrencyQuotationSummary
            {
                MinimumBidPrice = group.Min(quotation => quotation.BidPrice),
                MaximumBidPrice = group.Max(quotation => quotation.BidPrice),
                AverageBidPrice = group.Average(quotation => quotation.BidPrice),
                TotalQuotations = group.Count()
            })
            .FirstAsync(cancellationToken);
    }

    public async Task<CurrencyQuotation?> GetFirstAsync(CurrencyPair currencyPair,DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
    {
        var query = BuildFilterQuery(currencyPair, startDate, endDate);

        return await query
            .AsNoTracking()
            .OrderBy(quotation => quotation.QuotationDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private IQueryable<CurrencyQuotation> BuildFilterQuery(CurrencyPair? currencyPair, DateTime? startDate, DateTime? endDate)
    {
        IQueryable<CurrencyQuotation> query = _context.CurrencyQuotations;

        if (currencyPair is not null)
        {
            query = query.Where(quotation => quotation.CurrencyPair == currencyPair);
        }

        if (startDate.HasValue)
        {
            query = query.Where(quotation => quotation.QuotationDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(quotation => quotation.QuotationDate <= endDate.Value);
        }

        return query;
    }

    
}
