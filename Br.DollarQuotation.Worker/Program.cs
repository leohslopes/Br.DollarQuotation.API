using Br.DollarQuotation.CrossCutting.IoC;
using Br.DollarQuotation.Worker.Configurations;
using Br.DollarQuotation.Worker.Services;
using Br.DollarQuotation.Worker.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

var settings = new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
};

var builder = Host.CreateApplicationBuilder(settings);

builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile(
        "appsettings.json",
        optional: false,
        reloadOnChange: true)
    .AddJsonFile(
        $"appsettings.{builder.Environment.EnvironmentName}.json",
        optional: true,
        reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Configuration.AddUserSecrets<Program>(
    optional: false);


builder.Services.RegisterDependencies(
    builder.Configuration);

builder.Services.Configure<QuotationWorkerOptions>(
    builder.Configuration.GetSection(
        QuotationWorkerOptions.SectionName));

builder.Services.Configure<InternalApiOptions>(
    builder.Configuration.GetSection(
        InternalApiOptions.SectionName));

builder.Services.AddHttpClient<
    IQuotationNotificationClient,
    QuotationNotificationClient>(
    (serviceProvider, httpClient) =>
    {
        var options = serviceProvider
            .GetRequiredService<IOptions<InternalApiOptions>>()
            .Value;

        if (string.IsNullOrWhiteSpace(options.BaseUrl))
        {
            throw new InvalidOperationException(
                "A URL da API interna não foi configurada.");
        }

        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            throw new InvalidOperationException(
                "A chave da API interna não foi configurada.");
        }

        httpClient.BaseAddress =
            new Uri(options.BaseUrl);

        httpClient.Timeout =
            TimeSpan.FromSeconds(15);

        httpClient.DefaultRequestHeaders.Add(
            "X-Internal-Api-Key",
            options.ApiKey);
    });

builder.Services.AddHostedService<
    CurrencyQuotationWorker>();

var host = builder.Build();

await host.RunAsync();