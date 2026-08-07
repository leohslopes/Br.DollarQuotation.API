namespace Br.DollarQuotation.Repository.ExternalServices.AwesomeApi;

public sealed class AwesomeApiOptions
{
    public const string SectionName = "AwesomeApi";

    public string BaseUrl { get; set; } = string.Empty;

    public string? ApiKey { get; set; }
}