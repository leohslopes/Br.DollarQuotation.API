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
        // As configurações precisam existir
        // porque RegisterDependencies lê
        // RabbitMqOptions durante a inicialização.

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
                // =========================
                // AUTH SERVICE
                // =========================
                // Os testes do controller usam
                // um mock para não depender das
                // regras internas do AuthService.

                var authServiceDescriptor =
                    services.FirstOrDefault(
                        descriptor =>
                            descriptor.ServiceType ==
                            typeof(IAuthService));

                if (authServiceDescriptor is not null)
                {
                    services.Remove(
                        authServiceDescriptor);
                }

                services.AddScoped(
                    _ => AuthServiceMock.Object);

                // =========================
                // DESABILITAR CONSUMER
                // =========================
                // O teste de Auth não deve
                // depender de RabbitMQ.

                var consumerDescriptor =
                    services.FirstOrDefault(
                        descriptor =>
                            descriptor.ServiceType ==
                                typeof(IHostedService) &&
                            descriptor.ImplementationType ==
                                typeof(
                                    QuotationUpdatedConsumerWorker));

                if (consumerDescriptor is not null)
                {
                    services.Remove(
                        consumerDescriptor);
                }
            });
    }
}