namespace Br.DollarQuotation.Worker.Configurations;

public sealed class InternalApiOptions
{
    public const string SectionName = "InternalApi";

    public string BaseUrl { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;
}