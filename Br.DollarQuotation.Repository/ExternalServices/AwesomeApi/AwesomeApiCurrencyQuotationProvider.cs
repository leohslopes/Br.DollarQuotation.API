using System.Globalization;
using System.Net;
using System.Text.Json;
using Br.DollarQuotation.Domain.Entities;
using Br.DollarQuotation.Domain.Exceptions;
using Br.DollarQuotation.Domain.Interfaces.Services;
using Br.DollarQuotation.Domain.ValueObjects;
using Br.DollarQuotation.Repository.ExternalServices.AwesomeApi.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Br.DollarQuotation.Repository.ExternalServices.AwesomeApi;

public sealed class AwesomeApiCurrencyQuotationProvider : ICurrencyQuotationProvider
{
    private readonly HttpClient _httpClient;
    private readonly AwesomeApiOptions _options;
    private readonly ILogger<AwesomeApiCurrencyQuotationProvider> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AwesomeApiCurrencyQuotationProvider(
        HttpClient httpClient,
        IOptions<AwesomeApiOptions> options,
        ILogger<AwesomeApiCurrencyQuotationProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<CurrencyQuotation> GetCurrentAsync(CurrencyPair currencyPair, CancellationToken cancellationToken = default)
    {
        try
        {
            var pairCode = currencyPair.ToCode();
            var endpoint = $"json/last/{pairCode}";

            if (!string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                endpoint += $"?token={Uri.EscapeDataString(_options.ApiKey)}";
            }

            using var response = await SendRequestWithRetryAsync(currencyPair, endpoint,cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new QuotationProviderException(currencyPair,$"A fonte externa retornou o status HTTP {(int)response.StatusCode}.");
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            var quotations = JsonSerializer.Deserialize<Dictionary<string, AwesomeApiQuotationResponse>>(json, JsonOptions);

            if (quotations is null || quotations.Count == 0)
            {
                throw new QuotationProviderException(currencyPair, "A fonte externa retornou uma resposta vazia.");
            }

            var quotationResponse = quotations.Values.First();

            return MapToEntity(currencyPair, quotationResponse);
        }
        catch (QuotationProviderException)
        {
            throw;
        }
        catch (HttpRequestException exception)
        {
            throw new QuotationProviderException(currencyPair, "Não foi possível estabelecer conexão com a fonte externa.", exception);
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new QuotationProviderException(currencyPair, "A consulta à fonte externa excedeu o tempo limite.", exception);
        }
        catch (JsonException exception)
        {
            throw new QuotationProviderException(currencyPair, "A resposta da fonte externa possui um formato inválido.", exception);
        }
        catch (FormatException exception)
        {
            throw new QuotationProviderException(currencyPair, "A fonte externa retornou valores numéricos inválidos.",exception);
        }
    }

    public Task<IReadOnlyCollection<CurrencyQuotation>> GetHistoryAsync(CurrencyPair currencyPair, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("A consulta de histórico será implementada na próxima etapa.");
    }

    private async Task<HttpResponseMessage> SendRequestWithRetryAsync(CurrencyPair currencyPair,string endpoint, CancellationToken cancellationToken)
    {
        const int maximumAttempts = 4;

        for (var attempt = 1; attempt <= maximumAttempts; attempt++)
        {
            var response = await _httpClient.GetAsync(endpoint, cancellationToken);

            if (response.StatusCode != HttpStatusCode.TooManyRequests)
            {
                return response;
            }

            if (attempt == maximumAttempts)
            {
                _logger.LogWarning(
                    "A AwesomeAPI continuou retornando HTTP 429 após {Attempts} tentativa(s) para {CurrencyPair}.",
                    maximumAttempts,
                    currencyPair.ToCode()
                );

                return response;
            }

            var delay = GetRetryDelay( response,attempt);

            _logger.LogWarning(
                "HTTP 429 recebido da AwesomeAPI para {CurrencyPair}. " +
                "Tentativa {Attempt}/{MaximumAttempts}. " +
                "Nova tentativa em {DelayInSeconds:N1} segundo(s).",
                currencyPair.ToCode(),
                attempt,
                maximumAttempts,
                delay.TotalSeconds
            );

            response.Dispose();

            await Task.Delay(delay, cancellationToken);
        }

        throw new InvalidOperationException("Não foi possível concluir a consulta da cotação.");
    }

    private static TimeSpan GetRetryDelay(HttpResponseMessage response, int attempt)
    {
        var retryAfter = response.Headers.RetryAfter;

        if (retryAfter?.Delta is not null)
        {
            return retryAfter.Delta.Value;
        }

        if (retryAfter?.Date is not null)
        {
            var delay =retryAfter.Date.Value - DateTimeOffset.UtcNow;

            if (delay > TimeSpan.Zero)
            {
                return delay;
            }
        }

        var exponentialDelayInSeconds = Math.Pow(2, attempt);
        var jitterInMilliseconds = Random.Shared.Next(250, 1000);
         

        return TimeSpan.FromSeconds(exponentialDelayInSeconds)  + TimeSpan.FromMilliseconds(jitterInMilliseconds);
    }

    private static CurrencyQuotation MapToEntity(CurrencyPair currencyPair, AwesomeApiQuotationResponse response)
    {
        var quotationDate = ParseQuotationDate(response);

        return new CurrencyQuotation(
            currencyPair: currencyPair,
            bidPrice: ParseDecimal(response.Bid),
            askPrice: ParseDecimal(response.Ask),
            highPrice: ParseDecimal(response.High),
            lowPrice: ParseDecimal(response.Low),
            variation: ParseDecimal(response.Variation),
            variationPercentage: ParseDecimal(
                response.VariationPercentage
            ),
            quotationDate: quotationDate
        );
    }

    private static decimal ParseDecimal(string value)
    {
        if (!decimal.TryParse(value,NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
        {
            throw new FormatException( $"O valor '{value}' não é um decimal válido.");
        }

        return result;
    }

    private static DateTime ParseQuotationDate(
        AwesomeApiQuotationResponse response)
    {
        if (long.TryParse(response.Timestamp, NumberStyles.Integer,CultureInfo.InvariantCulture, out var unixTimestamp))
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).UtcDateTime;
        }

        if (DateTime.TryParse(response.CreateDate, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal,out var createDate))
        {
            return createDate.ToUniversalTime();
        }

        throw new FormatException( "A data da cotação retornada é inválida."
        );
    }
}