namespace Br.DollarQuotation.Application.DTOs.Responses;

public sealed class LoginResponse
{
    public Guid UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhotoBase64 { get; set; }

    public string? PhotoContentType { get; set; }

    public string AccessToken { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
}
