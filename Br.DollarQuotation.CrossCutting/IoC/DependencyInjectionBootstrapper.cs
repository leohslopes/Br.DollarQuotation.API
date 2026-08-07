using Br.DollarQuotation.Application.Interfaces.Services;
using Br.DollarQuotation.Application.Services;
using Br.DollarQuotation.Domain.Interfaces.Repositories;
using Br.DollarQuotation.Domain.Interfaces.Services;
using Br.DollarQuotation.Repository.Configurations;
using Br.DollarQuotation.Repository.Context;
using Br.DollarQuotation.Repository.ExternalServices.AwesomeApi;
using Br.DollarQuotation.Repository.Repositories;
using Br.DollarQuotation.Repository.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Br.DollarQuotation.CrossCutting.IoC;

public static class DependencyInjectionBootstrapper
{
    public static IServiceCollection RegisterDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        RegisterDatabase(services, configuration);
        RegisterRepositories(services);
        RegisterApplicationServices(services);
        RegisterInfrastructureServices(services);
        RegisterExternalServices(services, configuration);
        
        return services;
    }

    public static IServiceCollection RegisterJwtConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        RegisterJwtOptions(services, configuration);
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
    
        return services;
    }

    private static void RegisterDatabase(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("A connection string 'DefaultConnection' não foi configurada.");
        }

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });
    }

    private static void RegisterRepositories(IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICurrencyQuotationRepository, CurrencyQuotationRepository>();
    }

    private static void RegisterApplicationServices(IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICurrencyQuotationService, CurrencyQuotationService>();
    }

    private static void RegisterInfrastructureServices(IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, PasswordHasher>();
    }

    private static void RegisterExternalServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AwesomeApiOptions>(configuration.GetSection(AwesomeApiOptions.SectionName));

        services.AddHttpClient<ICurrencyQuotationProvider, AwesomeApiCurrencyQuotationProvider>(
            (serviceProvider, httpClient) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<AwesomeApiOptions>>().Value;

                if (string.IsNullOrWhiteSpace(options.BaseUrl))
                {
                    throw new InvalidOperationException( "A URL da AwesomeAPI não foi configurada.");
                }

                httpClient.BaseAddress = new Uri(options.BaseUrl);
                httpClient.Timeout = TimeSpan.FromSeconds(15);
            });
    }

    private static void RegisterJwtOptions(IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection(JwtOptions.SectionName);

        services.Configure<JwtOptions>(jwtSection);

        var jwtOptions = jwtSection.Get<JwtOptions>() ?? throw new InvalidOperationException("As configurações do JWT não foram encontradas.");

        if (string.IsNullOrWhiteSpace(jwtOptions.SecretKey))
        {
            throw new InvalidOperationException("A chave secreta do JWT não foi configurada.");
        }

        if (jwtOptions.SecretKey.Length < 32)
        {
            throw new InvalidOperationException("A chave secreta do JWT deve possuir pelo menos 32 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(jwtOptions.Issuer))
        {
            throw new InvalidOperationException("O emissor do JWT não foi configurado.");
        }

        if (string.IsNullOrWhiteSpace(jwtOptions.Audience))
        {
            throw new InvalidOperationException("O público do JWT não foi configurado.");
        }

        if (jwtOptions.ExpirationInMinutes <= 0)
        {
            throw new InvalidOperationException("O tempo de expiração do JWT deve ser maior que zero.");
        }
    }
}