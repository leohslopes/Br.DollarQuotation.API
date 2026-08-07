using System.Text.Json.Serialization;

namespace Br.DollarQuotation.Repository.ExternalServices.AwesomeApi.Models;

public sealed class AwesomeApiQuotationResponse
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("codein")]
    public string CodeIn { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("high")]
    public string High { get; set; } = string.Empty;

    [JsonPropertyName("low")]
    public string Low { get; set; } = string.Empty;

    [JsonPropertyName("varBid")]
    public string Variation { get; set; } = string.Empty;

    [JsonPropertyName("pctChange")]
    public string VariationPercentage { get; set; } = string.Empty;

    [JsonPropertyName("bid")]
    public string Bid { get; set; } = string.Empty;

    [JsonPropertyName("ask")]
    public string Ask { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;

    [JsonPropertyName("create_date")]
    public string CreateDate { get; set; } = string.Empty;
}