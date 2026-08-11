using Br.DollarQuotation.Application.DTOs.Requests;
using Br.DollarQuotation.Application.DTOs.Responses;
using Br.DollarQuotation.Application.Interfaces.Services;
using Br.DollarQuotation.Domain.Entities;
using Br.DollarQuotation.Domain.Exceptions;
using Br.DollarQuotation.Domain.Interfaces.Repositories;
using Br.DollarQuotation.Domain.ValueObjects;

namespace Br.DollarQuotation.Application.Services;

public sealed class QuotationAlertService : IQuotationAlertService
{
    private readonly IQuotationAlertRepository _quotationAlertRepository;

    public QuotationAlertService(
        IQuotationAlertRepository quotationAlertRepository)
    {
        _quotationAlertRepository = quotationAlertRepository;
    }

    public async Task<QuotationAlertResponse> CreateAsync(
        Guid userId,
        CreateQuotationAlertRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (userId == Guid.Empty)
        {
            throw new DomainException(
                "O usuário autenticado é inválido."
            );
        }

        var currencyPair = CurrencyPair.Create(
            request.BaseCurrency,
            request.QuoteCurrency
        );

        var alert = new QuotationAlert(
            userId,
            currencyPair,
            request.Condition,
            request.TargetPrice
        );

        await _quotationAlertRepository.AddAsync(
            alert,
            cancellationToken
        );

        await _quotationAlertRepository.SaveChangesAsync(
            cancellationToken
        );

        return MapToResponse(alert);
    }

    public async Task<IReadOnlyCollection<QuotationAlertResponse>> GetByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException(
                "O usuário autenticado é inválido."
            );
        }

        var alerts = await _quotationAlertRepository.GetByUserIdAsync(
            userId,
            cancellationToken
        );

        return alerts
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<QuotationAlertResponse> ActivateAsync(
        Guid userId,
        Guid alertId,
        CancellationToken cancellationToken = default)
    {
        var alert = await GetUserAlertAsync(
            userId,
            alertId,
            cancellationToken
        );

        alert.Activate();

        await _quotationAlertRepository.SaveChangesAsync(
            cancellationToken
        );

        return MapToResponse(alert);
    }

    public async Task<QuotationAlertResponse> DeactivateAsync(
        Guid userId,
        Guid alertId,
        CancellationToken cancellationToken = default)
    {
        var alert = await GetUserAlertAsync(
            userId,
            alertId,
            cancellationToken
        );

        alert.Deactivate();

        await _quotationAlertRepository.SaveChangesAsync(
            cancellationToken
        );

        return MapToResponse(alert);
    }

    private async Task<QuotationAlert> GetUserAlertAsync(
        Guid userId,
        Guid alertId,
        CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException(
                "O usuário autenticado é inválido."
            );
        }

        if (alertId == Guid.Empty)
        {
            throw new DomainException(
                "O identificador do alerta é inválido."
            );
        }

        var alert = await _quotationAlertRepository.GetByIdAsync(
            alertId,
            cancellationToken
        );

        if (alert is null || alert.UserId != userId)
        {
            throw new DomainException(
                "Alerta de cotação não encontrado."
            );
        }

        return alert;
    }

    private static QuotationAlertResponse MapToResponse(
        QuotationAlert alert)
    {
        return new QuotationAlertResponse
        {
            Id = alert.Id,

            UserId = alert.UserId,

            BaseCurrency =
                alert.CurrencyPair.BaseCurrency.ToString(),

            QuoteCurrency =
                alert.CurrencyPair.QuoteCurrency.ToString(),

            CurrencyPair =
                alert.CurrencyPair.ToCode(),

            Condition =
                alert.Condition,

            TargetPrice =
                alert.TargetPrice,

            IsActive =
                alert.IsActive,

            TriggeredAt =
                alert.TriggeredAt,

            CreatedAt =
                alert.CreatedAt,

            UpdatedAt =
                alert.UpdatedAt
        };
    }
}