using Br.DollarQuotation.Domain.Common;
using Br.DollarQuotation.Domain.Exceptions;

namespace Br.DollarQuotation.Domain.Entities;

public sealed class PasswordResetToken : Entity
{
    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; } =
        string.Empty;

    public DateTime ExpiresAt { get; private set; }

    public DateTime? UsedAt { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public bool IsUsed =>
        UsedAt.HasValue;

    public bool IsExpired =>
        DateTime.UtcNow >= ExpiresAt;

    public bool IsValid =>
        !IsUsed &&
        !IsExpired;

    private PasswordResetToken()
    {
    }

    public PasswordResetToken(
        Guid userId,
        string tokenHash,
        DateTime expiresAt)
    {
        SetUserId(userId);
        SetTokenHash(tokenHash);
        SetExpiration(expiresAt);

        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsUsed()
    {
        if (IsUsed)
        {
            throw new DomainException("O token de recuperação de senha já foi utilizado.");
        }

        UsedAt = DateTime.UtcNow;
    }

    private void SetUserId(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("O usuário do token de recuperação é obrigatório.");
        }

        UserId = userId;
    }

    private void SetTokenHash(string tokenHash)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            throw new DomainException("O hash do token de recuperação é obrigatório.");
        }

        TokenHash = tokenHash.Trim();
    }

    private void SetExpiration(DateTime expiresAt)
    {
        var expiration = expiresAt.Kind == DateTimeKind.Utc ? expiresAt : expiresAt.ToUniversalTime();

        if (expiration <= DateTime.UtcNow)
        {
            throw new DomainException( "A data de expiração do token deve ser futura.");
        }

        ExpiresAt = expiration;
    }
}