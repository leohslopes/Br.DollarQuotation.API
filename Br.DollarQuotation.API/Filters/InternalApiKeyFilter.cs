using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Br.DollarQuotation.API.Filters;

public sealed class InternalApiKeyFilter : IAsyncAuthorizationFilter
{
    private const string ApiKeyHeaderName = "X-Internal-Api-Key";

    private readonly IConfiguration _configuration;
    private readonly ILogger<InternalApiKeyFilter> _logger;

    public InternalApiKeyFilter(IConfiguration configuration,
        ILogger<InternalApiKeyFilter> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var configuredApiKey = _configuration["InternalApi:ApiKey"];

        if (string.IsNullOrWhiteSpace(configuredApiKey))
        {
            throw new InvalidOperationException("A chave da API interna não foi configurada.");
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var receivedApiKey))
        {
            SetUnauthorizedResult(context);
            return Task.CompletedTask;
        }

        var isValid = FixedTimeEquals(receivedApiKey.ToString(),configuredApiKey);

        if (!isValid)
        {
            _logger.LogWarning("Tentativa de acesso ao endpoint interno com chave inválida. Path: {Path}", context.HttpContext.Request.Path);

            SetUnauthorizedResult(context);
        }

        return Task.CompletedTask;
    }

    private static void SetUnauthorizedResult(AuthorizationFilterContext context)
    {
        context.Result = new UnauthorizedObjectResult(
            new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Acesso não autorizado",
                Detail = "A chave da API interna é inválida ou não foi informada.",
                Instance = context.HttpContext.Request.Path,
                Extensions =
                {
                    ["traceId"] =
                        context.HttpContext.TraceIdentifier
                }
            });
    }

    private static bool FixedTimeEquals(string receivedApiKey, string configuredApiKey)
    {
        var receivedBytes = Encoding.UTF8.GetBytes(receivedApiKey);
        var configuredBytes =  Encoding.UTF8.GetBytes(configuredApiKey);

        return receivedBytes.Length == configuredBytes.Length && CryptographicOperations.FixedTimeEquals(receivedBytes, configuredBytes);
    }
}