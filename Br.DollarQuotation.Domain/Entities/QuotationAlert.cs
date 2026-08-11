using Br.DollarQuotation.Domain.Common;
using Br.DollarQuotation.Domain.Enums;
using Br.DollarQuotation.Domain.Exceptions;
using Br.DollarQuotation.Domain.ValueObjects;

namespace Br.DollarQuotation.Domain.Entities;

public class QuotationAlert : Entity
{
    public Guid UserId { get; private set; }

    public CurrencyPair CurrencyPair { get; private set; } = null!;

    public AlertCondition Condition { get; private set; }

    public decimal TargetPrice { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime? TriggeredAt { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    protected QuotationAlert()
    {
    }

    public QuotationAlert(
        Guid userId,
        CurrencyPair currencyPair,
        AlertCondition condition,
        decimal targetPrice)
    {
        SetUserId(userId);
        SetCurrencyPair(currencyPair);
        SetCondition(condition);
        SetTargetPrice(targetPrice);

        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(
        CurrencyPair currencyPair,
        AlertCondition condition,
        decimal targetPrice)
    {
        SetCurrencyPair(currencyPair);
        SetCondition(condition);
        SetTargetPrice(targetPrice);

        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        TriggeredAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool ShouldTrigger(decimal currentPrice)
    {
        if (!IsActive)
        {
            return false;
        }

        if (currentPrice <= 0)
        {
            throw new DomainException(
                "O preço atual deve ser maior que zero."
            );
        }

        return Condition switch
        {
            AlertCondition.Above => currentPrice >= TargetPrice,
            AlertCondition.Below => currentPrice <= TargetPrice,
            _ => false
        };
    }

    public void MarkAsTriggered()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        TriggeredAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    private void SetUserId(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException(
                "O usuário do alerta é obrigatório."
            );
        }

        UserId = userId;
    }

    private void SetCurrencyPair(CurrencyPair currencyPair)
    {
        CurrencyPair = currencyPair
            ?? throw new DomainException(
                "O par de moedas do alerta é obrigatório."
            );
    }

    private void SetCondition(AlertCondition condition)
    {
        if (!Enum.IsDefined(condition))
        {
            throw new DomainException(
                "A condição do alerta é inválida."
            );
        }

        Condition = condition;
    }

    private void SetTargetPrice(decimal targetPrice)
    {
        if (targetPrice <= 0)
        {
            throw new DomainException(
                "O valor alvo do alerta deve ser maior que zero."
            );
        }

        TargetPrice = targetPrice;
    }
}