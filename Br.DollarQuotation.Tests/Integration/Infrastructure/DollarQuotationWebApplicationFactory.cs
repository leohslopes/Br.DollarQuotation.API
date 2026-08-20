using Br.DollarQuotation.API.Services;
using Br.DollarQuotation.Application.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace Br.DollarQuotation.Tests.Integration.Infrastructure;

public sealed class DollarQuotationWebApplicationFactory
    : WebApplicationFactory<Program>
{
    public Mock<IAuthService> AuthServiceMock { get; } =
        new();

    public Mock<IUserService> UserServiceMock { get; } =
        new();

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment(
            "Testing");

        // =============================
        // CONFIGURAÇÕES DE TESTE
        // =============================

        var testConnectionString =
            Environment.GetEnvironmentVariable(
                "TEST_DB_CONNECTION_STRING")
            ??
            "Host=localhost;" +
            "Port=5432;" +
            "Database=dollar_quotation_test;" +
            "Username=postgres;" +
            "Password=123456";

        builder.UseSetting(
            "ConnectionStrings:DefaultConnection",
            testConnectionString);

        builder.UseSetting(
            "Jwt:SecretKey",
            "BrDollarQuotation@TestJwtKey#2026!123456789");

        builder.UseSetting(
            "Jwt:Issuer",
            "Br.DollarQuotation.Tests");

        builder.UseSetting(
            "Jwt:Audience",
            "Br.DollarQuotation.Tests");

        builder.UseSetting(
            "Jwt:ExpirationInMinutes",
            "60");

        // =============================
        // RABBITMQ
        // =============================

        builder.UseSetting(
            "RabbitMq:HostName",
            "localhost");

        builder.UseSetting(
            "RabbitMq:Port",
            "5672");

        builder.UseSetting(
            "RabbitMq:UserName",
            "guest");

        builder.UseSetting(
            "RabbitMq:Password",
            "guest");

        builder.UseSetting(
            "RabbitMq:VirtualHost",
            "/");

        builder.UseSetting(
            "RabbitMq:ExchangeName",
            "dollarquotation.exchange.test");

        builder.UseSetting(
            "RabbitMq:QuotationQueueName",
            "dollarquotation.quotation.queue.test");

        builder.UseSetting(
            "RabbitMq:QuotationRoutingKey",
            "quotation.updated");

        // =============================
        // SERVIÇOS DE TESTE
        // =============================

        builder.ConfigureServices(
            services =>
            {
                ConfigureAuthService(
                    services);

                ConfigureUserService(
                    services);

                DisableQuotationConsumer(
                    services);
            });
    }

    private void ConfigureAuthService(
        IServiceCollection services)
    {
        var descriptor =
            services.FirstOrDefault(
                service =>
                    service.ServiceType ==
                    typeof(IAuthService));

        if (descriptor is not null)
        {
            services.Remove(
                descriptor);
        }

        services.AddScoped(
            _ =>
                AuthServiceMock.Object);
    }

    private void ConfigureUserService(
        IServiceCollection services)
    {
        var descriptor =
            services.FirstOrDefault(
                service =>
                    service.ServiceType ==
                    typeof(IUserService));

        if (descriptor is not null)
        {
            services.Remove(
                descriptor);
        }

        services.AddScoped(
            _ =>
                UserServiceMock.Object);
    }

    private static void DisableQuotationConsumer(
        IServiceCollection services)
    {
        var descriptor =
            services.FirstOrDefault(
                service =>
                    service.ServiceType ==
                        typeof(IHostedService) &&
                    service.ImplementationType ==
                        typeof(
                            QuotationUpdatedConsumerWorker));

        if (descriptor is not null)
        {
            services.Remove(
                descriptor);
        }
    }
}