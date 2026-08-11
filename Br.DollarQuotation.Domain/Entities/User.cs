using Br.DollarQuotation.Domain.Common;
using Br.DollarQuotation.Domain.Exceptions;
using Br.DollarQuotation.Domain.ValueObjects;

namespace Br.DollarQuotation.Domain.Entities;

public class User : Entity
{
    private const int MinimumNameLength = 3;
    private const int MaximumNameLength = 150;
    private const int MaxPhotoSizeInBytes = 2 * 1024 * 1024;

    private static readonly string[] AllowedContentTypes =
    [
        "image/png",
        "image/jpeg",
        "image/webp"
    ];

    public string Name { get; private set; } = string.Empty;

    public Email Email { get; private set; } = null!;

    public string PasswordHash { get; private set; } = string.Empty;

    public string? PhotoBase64 { get; private set; }

    public string? PhotoContentType { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    protected User()
    {
    }

    public User(
        string name,
        Email email,
        string passwordHash,
        string? photoBase64 = null,
        string? photoContentType = null)
    {
        SetName(name);
        SetEmail(email);
        SetPasswordHash(passwordHash);

        CreatedAt = DateTime.UtcNow;
        IsActive = true;

        ConfigurePhoto(photoBase64, photoContentType);
    }

    public void UpdateName(string name)
    {
        SetName(name);
        SetUpdatedAt();
    }

    public void UpdateEmail(Email email)
    {
        SetEmail(email);
        SetUpdatedAt();
    }

    public void UpdatePassword(string passwordHash)
    {
        SetPasswordHash(passwordHash);
        SetUpdatedAt();
    }

    public void UpdatePhoto(string photoBase64, string photoContentType)
    {
        if (string.IsNullOrWhiteSpace(photoBase64))
        {
            throw new DomainException("A foto é obrigatória.");
        }

        if (string.IsNullOrWhiteSpace(photoContentType))
        {
            throw new DomainException("O tipo da imagem é obrigatório.");
        }

        ValidatePhotoContentType(photoContentType);

        var normalizedBase64 = NormalizeBase64(photoBase64);

        byte[] imageBytes;

        try
        {
            imageBytes = Convert.FromBase64String(normalizedBase64);
        }
        catch (FormatException)
        {
            throw new DomainException("A imagem informada não possui um Base64 válido.");
        }

        if (imageBytes.Length > MaxPhotoSizeInBytes)
        {
            throw new DomainException( "A foto deve possuir no máximo 2 MB.");
        }

        PhotoBase64 = normalizedBase64;

        PhotoContentType = photoContentType.Trim();

        SetUpdatedAt();
    }

    public void RemovePhoto()
    {
        PhotoBase64 = null;
        PhotoContentType = null;

        SetUpdatedAt();
    }

    public void Activate()
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;

        SetUpdatedAt();
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;

        SetUpdatedAt();
    }

    private void ConfigurePhoto(string? photoBase64, string? photoContentType)
    {
        var hasPhoto = !string.IsNullOrWhiteSpace(photoBase64);
        var hasContentType =!string.IsNullOrWhiteSpace(photoContentType);

        if (!hasPhoto && !hasContentType)
        {
            return;
        }

        if (!hasPhoto)
        {
            throw new DomainException("A foto é obrigatória quando o tipo da imagem é informado.");
        }

        if (!hasContentType)
        {
            throw new DomainException( "O tipo da imagem é obrigatório quando a foto é informada.");
        }

        UpdatePhoto(photoBase64!, photoContentType!);
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException( "O nome é obrigatório.");
        }

        var normalizedName = name.Trim();

        if (normalizedName.Length < MinimumNameLength)
        {
            throw new DomainException( $"O nome deve possuir no mínimo {MinimumNameLength} caracteres.");
        }

        if (normalizedName.Length > MaximumNameLength)
        {
            throw new DomainException( $"O nome deve possuir no máximo {MaximumNameLength} caracteres.");
        }

        Name = normalizedName;
    }

    private void SetEmail(Email email)
    {
        Email = email ?? throw new DomainException("O e-mail é obrigatório.");
    }

    private void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException("O hash da senha é obrigatório.");
        }

        PasswordHash = passwordHash;
    }

    private static void ValidatePhotoContentType(string photoContentType)
    {
        var isAllowed = AllowedContentTypes.Contains(photoContentType, StringComparer.OrdinalIgnoreCase);

        if (!isAllowed)
        {
            throw new DomainException("Formato de imagem não permitido. Utilize PNG, JPEG ou WEBP.");
        }
    }

    private void SetUpdatedAt()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    private static string NormalizeBase64(string photoBase64)
    {
        var normalizedBase64 = photoBase64.Trim();

        var commaIndex = normalizedBase64.IndexOf(',');

        if ( normalizedBase64.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && commaIndex >= 0)
        {
            normalizedBase64 = normalizedBase64[(commaIndex + 1)..];
        }

        try
        {
            Convert.FromBase64String(normalizedBase64);
        }
        catch (FormatException)
        {
            throw new DomainException("O conteúdo da foto não possui um Base64 válido.");
        }

        return normalizedBase64;
    }
}