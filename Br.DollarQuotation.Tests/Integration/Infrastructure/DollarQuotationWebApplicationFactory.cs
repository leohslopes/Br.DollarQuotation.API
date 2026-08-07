using Br.DollarQuotation.Application.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace Br.DollarQuotation.Tests.Integration.Infrastructure;

public sealed class DollarQuotationWebApplicationFactory
    : WebApplicationFactory<Program>
{
    public Mock<IAuthService> AuthServiceMock { get; } = new();

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration(
            (_, configuration) =>
            {
                var settings =
                    new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] =
                            "Host=localhost;Port=5432;Database=dollar_quotation_test;Username=postgres;Password=postgres",

                        ["Jwt:SecretKey"] =
                            "BrDollarQuotation@TestJwtKey#2026!123456789",

                        ["Jwt:Issuer"] =
                            "Br.DollarQuotation.Tests",

                        ["Jwt:Audience"] =
                            "Br.DollarQuotation.Tests",

                        ["Jwt:ExpirationInMinutes"] =
                            "60"
                    };

                configuration.AddInMemoryCollection(
                    settings);
            });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IAuthService>();

            services.AddScoped(
                _ => AuthServiceMock.Object);
        });
    }
}