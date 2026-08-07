using Br.DollarQuotation.Application.DTOs.Requests;
using Br.DollarQuotation.Application.DTOs.Responses;
using Br.DollarQuotation.Application.Interfaces.Services;
using Br.DollarQuotation.Domain.Entities;
using Br.DollarQuotation.Domain.Exceptions;
using Br.DollarQuotation.Domain.Interfaces.Repositories;
using Br.DollarQuotation.Domain.Interfaces.Services;
using Br.DollarQuotation.Domain.ValueObjects;

namespace Br.DollarQuotation.Application.Services;

public sealed class CurrencyQuotationService: ICurrencyQuotationService
{
    private readonly ICurrencyQuotationProvider _quotationProvider;
    private readonly ICurrencyQuotationRepository _quotationRepository;

    public CurrencyQuotationService( ICurrencyQuotationProvider quotationProvider,
        ICurrencyQuotationRepository quotationRepository)
    {
        _quotationProvider = quotationProvider;
        _quotationRepository = quotationRepository;
    }

    public async Task<CurrencyQuotationResponse> GetCurrentAsync(GetCurrentQuotationRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var currencyPair = CurrencyPair.Create(request.BaseCurrency, request.QuoteCurrency);
        var quotation = await _quotationProvider.GetCurrentAsync(currencyPair, cancellationToken);
        var quotationAlreadyExists = await _quotationRepository.ExistsAsync(quotation.CurrencyPair, quotation.QuotationDate, cancellationToken);

        var wasInserted = false;

        if (!quotationAlreadyExists)
        {
            await _quotationRepository.AddAsync(quotation, cancellationToken);

            wasInserted = true;
        }

        return MapToResponse(quotation, wasInserted);

    }

    public async Task<IReadOnlyCollection<CurrencyQuotationResponse>> GetHistoryAsync(GetQuotationHistoryRequest request, CancellationToken cancellationToken = default)
    {
        ValidateHistoryRequest(request);

        var currencyPair = CurrencyPair.Create(request.BaseCurrency,request.QuoteCurrency);
        var startDate = NormalizeUtc(request.StartDate);
        var endDate = NormalizeUtc(request.EndDate);

        var quotations = await _quotationRepository.GetHistoryAsync(currencyPair, startDate, endDate, cancellationToken);

        return quotations.Select(quotation => MapToResponse(quotation, wasInserted: false)).ToList().AsReadOnly();
    }

    public async Task<PagedResponse<CurrencyQuotationResponse>> GetPagedAsync(GetQuotationPagedRequest request, CancellationToken cancellationToken = default)
    {
        ValidatePagedRequest(request);

        CurrencyPair? currencyPair = null;

        if (!string.IsNullOrWhiteSpace(request.BaseCurrency) || !string.IsNullOrWhiteSpace(request.QuoteCurrency))
        {
            if (string.IsNullOrWhiteSpace(request.BaseCurrency) || string.IsNullOrWhiteSpace(request.QuoteCurrency))
            {
                throw new DomainException("A moeda base e a moeda de cotação devem ser informadas juntas.");
            }

            currencyPair = CurrencyPair.Create(request.BaseCurrency,request.QuoteCurrency);
        }

        DateTime? startDate = request.StartDate.HasValue ? NormalizeUtc(request.StartDate.Value): null;
        DateTime? endDate = request.EndDate.HasValue ? NormalizeUtc(request.EndDate.Value) : null;

        var quotations = await _quotationRepository.GetPagedAsync(
            currencyPair,
            startDate,
            endDate,
            request.Page,
            request.PageSize,
            cancellationToken);

        var totalItems = await _quotationRepository.CountAsync(
            currencyPair,
            startDate,
            endDate,
            cancellationToken);

        var totalPages = totalItems == 0
            ? 0
            : (int)Math.Ceiling(
                totalItems / (double)request.PageSize);

        return new PagedResponse<CurrencyQuotationResponse>
        {
            Items = quotations
                .Select(quotation =>
                    MapToResponse(
                        quotation,
                        wasInserted: false))
                .ToList()
                .AsReadOnly(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };
    }

    public async Task<CurrencyQuotationSummaryResponse> GetSummaryAsync(GetQuotationSummaryRequest request, CancellationToken cancellationToken = default)
    {
        ValidateSummaryRequest(request);

        var currencyPair = CurrencyPair.Create(request.BaseCurrency, request.QuoteCurrency);

        DateTime? startDate = request.StartDate.HasValue ? NormalizeUtc(request.StartDate.Value) : null;
        DateTime? endDate = request.EndDate.HasValue ? NormalizeUtc(request.EndDate.Value) : null;

        var summary = await _quotationRepository.GetSummaryAsync(currencyPair, startDate, endDate, cancellationToken) ?? throw new QuotationNotFoundException(currencyPair);
        var latestQuotations = await _quotationRepository.GetPagedAsync(currencyPair, startDate, endDate, page: 1, pageSize: 1, cancellationToken);
        var latestQuotation = latestQuotations.FirstOrDefault() ?? throw new QuotationNotFoundException(currencyPair);
        var firstQuotation = await _quotationRepository.GetFirstAsync(currencyPair, startDate, endDate, cancellationToken) ?? throw new QuotationNotFoundException(currencyPair);
        var variationPercentage = firstQuotation.BidPrice == 0 ? 0 : ((latestQuotation.BidPrice - firstQuotation.BidPrice) / firstQuotation.BidPrice) * 100;

        return new CurrencyQuotationSummaryResponse
        {
            BaseCurrency = currencyPair.BaseCurrency.ToString(),
            QuoteCurrency = currencyPair.QuoteCurrency.ToString(),
            CurrencyPair = currencyPair.ToDisplay(),
            LatestBidPrice = latestQuotation.BidPrice,
            LatestAskPrice = latestQuotation.AskPrice,
            MinimumBidPrice = summary.MinimumBidPrice,
            MaximumBidPrice = summary.MaximumBidPrice,
            AverageBidPrice = summary.AverageBidPrice,
            VariationPercentage = Math.Round(variationPercentage, 4),
            LatestQuotationDate = latestQuotation.QuotationDate,
            TotalQuotations = summary.TotalQuotations
        };
    }

    private static void ValidateSummaryRequest(GetQuotationSummaryRequest request)
    {
        if (request is null)
        {
            throw new DomainException("Os dados da consulta são obrigatórios.");
        }

        if (string.IsNullOrWhiteSpace(request.BaseCurrency))
        {
            throw new DomainException("A moeda base é obrigatória.");
        }

        if (string.IsNullOrWhiteSpace(request.QuoteCurrency))
        {
            throw new DomainException("A moeda de cotação é obrigatória.");
        }

        if (request.StartDate.HasValue &&
            request.EndDate.HasValue &&
            request.StartDate.Value > request.EndDate.Value)
        {
            throw new DomainException("A data inicial não pode ser maior que a data final.");
        }
    }

    private static void ValidatePagedRequest(GetQuotationPagedRequest request)
    {
        if (request is null)
        {
            throw new DomainException("Os dados da consulta são obrigatórios.");
        }

        if (request.Page <= 0)
        {
            throw new DomainException("A página deve ser maior que zero.");
        }

        if (request.PageSize <= 0 || request.PageSize > 100)
        {
            throw new DomainException("O tamanho da página deve estar entre 1 e 100.");
        }

        if (request.StartDate.HasValue &&
            request.EndDate.HasValue &&
            request.StartDate.Value > request.EndDate.Value)
        {
            throw new DomainException("A data inicial não pode ser maior que a data final.");
        }
    }

    private static CurrencyQuotationResponse MapToResponse(CurrencyQuotation quotation, bool wasInserted = false)
    {
        return new CurrencyQuotationResponse
        {
            Id = quotation.Id,
            BaseCurrency = quotation.CurrencyPair.BaseCurrency.ToString(),
            QuoteCurrency = quotation.CurrencyPair.QuoteCurrency.ToString(),
            CurrencyPair = quotation.CurrencyPair.ToDisplay(),
            BidPrice = quotation.BidPrice,
            AskPrice = quotation.AskPrice,
            HighPrice = quotation.HighPrice,
            LowPrice = quotation.LowPrice,
            Variation = quotation.Variation,
            VariationPercentage = quotation.VariationPercentage,
            QuotationDate = quotation.QuotationDate,
            CreatedAt = quotation.CreatedAt,
            WasInserted = wasInserted
        };
    }

    private static void ValidateRequest(GetCurrentQuotationRequest request)
    {
        if (request is null)
        {
            throw new DomainException("Os dados da consulta são obrigatórios.");
        }

        if (string.IsNullOrWhiteSpace(request.BaseCurrency))
        {
            throw new DomainException("A moeda base é obrigatória.");
        }

        if (string.IsNullOrWhiteSpace(request.QuoteCurrency))
        {
            throw new DomainException("A moeda de cotação é obrigatória.");
        }
    }

    private static void ValidateHistoryRequest(GetQuotationHistoryRequest request)
    {
        if (request is null)
        {
            throw new DomainException( "Os dados da consulta são obrigatórios.");
        }

        if (string.IsNullOrWhiteSpace(request.BaseCurrency))
        {
            throw new DomainException("A moeda base é obrigatória.");
        }

        if (string.IsNullOrWhiteSpace(request.QuoteCurrency))
        {
            throw new DomainException("A moeda de cotação é obrigatória.");
        }

        if (request.StartDate == default)
        {
            throw new DomainException("A data inicial é obrigatória.");
        }

        if (request.EndDate == default)
        {
            throw new DomainException("A data final é obrigatória.");
        }

        if (request.StartDate > request.EndDate)
        {
            throw new DomainException("A data inicial não pode ser maior que a data final.");
        }

        var maximumPeriod = TimeSpan.FromDays(366);

        if (request.EndDate - request.StartDate > maximumPeriod)
        {
            throw new DomainException( "O período máximo permitido para consulta é de 366 dias.");
        }
    }

    private static DateTime NormalizeUtc(DateTime date)
    {
        return date.Kind switch
        {
            DateTimeKind.Utc => date,
            DateTimeKind.Local => date.ToUniversalTime(),
            _ => DateTime.SpecifyKind(
                date,
                DateTimeKind.Utc)
        };
    }
}