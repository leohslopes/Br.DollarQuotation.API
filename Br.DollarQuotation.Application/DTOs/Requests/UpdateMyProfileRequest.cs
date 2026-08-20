namespace Br.DollarQuotation.Application.DTOs.Requests;

public sealed class UpdateMyProfileRequest
{
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}