namespace Br.DollarQuotation.Application.DTOs.Requests;

public sealed class UpdateUserPhotoRequest
{
    public string PhotoBase64 { get; set; } = string.Empty;

    public string PhotoContentType { get; set; } = string.Empty;
}