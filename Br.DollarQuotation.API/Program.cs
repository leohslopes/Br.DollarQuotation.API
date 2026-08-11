using Br.DollarQuotation.API.Filters;
using Br.DollarQuotation.API.Hubs;
using Br.DollarQuotation.API.Middlewares;
using Br.DollarQuotation.API.Services;
using Br.DollarQuotation.API.Services.Interfaces;
using Br.DollarQuotation.CrossCutting.IoC;
using Br.DollarQuotation.Repository.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicyName = "AngularPolicy";

// Controllers
builder.Services.AddControllers();

// Padronização dos erros de validação do [ApiController]
builder.Services.Configure<ApiBehaviorOptions>(
    options =>
    {
        options.InvalidModelStateResponseFactory =
            context =>
            {
                var errors =
                    context.ModelState
                        .Where(
                            item =>
                                item.Value?.Errors.Count > 0)
                        .ToDictionary(
                            item => item.Key,
                            item => item.Value!.Errors
                                .Select(
                                    error =>
                                        string.IsNullOrWhiteSpace(
                                            error.ErrorMessage)
                                            ? "O valor informado é inválido."
                                            : error.ErrorMessage)
                                .ToArray());

                var problemDetails =
                    new ValidationProblemDetails(
                        errors)
                    {
                        Status =
                            StatusCodes
                                .Status400BadRequest,

                        Title =
                            "Erro de validação",

                        Detail =
                            "Um ou mais dados da requisição são inválidos.",

                        Instance =
                            context.HttpContext
                                .Request
                                .Path
                    };

                problemDetails
                    .Extensions["traceId"] =
                    context.HttpContext
                        .TraceIdentifier;

                return new BadRequestObjectResult(
                    problemDetails);
            };
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(
    options =>
    {
        options.SwaggerDoc(
            "v1",
            new OpenApiInfo
            {
                Title =
                    "Br.DollarQuotation.API",

                Version =
                    "v1"
            });

        options.AddSecurityDefinition(
            "Bearer",
            new OpenApiSecurityScheme
            {
                Name =
                    "Authorization",

                Type =
                    SecuritySchemeType.Http,

                Scheme =
                    "bearer",

                BearerFormat =
                    "JWT",

                In =
                    ParameterLocation.Header,

                Description =
                    "Informe somente o token JWT, sem escrever a palavra Bearer."
            });

        options.AddSecurityRequirement(
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference =
                            new OpenApiReference
                            {
                                Type =
                                    ReferenceType
                                        .SecurityScheme,

                                Id =
                                    "Bearer"
                            }
                    },
                    Array.Empty<string>()
                }
            });
    });

// =============================
// SIGNALR
// =============================

builder.Services.AddSignalR();

// =============================
// SERVIÇOS ESPECÍFICOS DA API
// =============================

builder.Services.AddScoped<
    IQuotationNotificationService,
    QuotationNotificationService>();

builder.Services.AddScoped<
    InternalApiKeyFilter>();

// =============================
// RABBITMQ CONSUMER
// =============================

builder.Services.AddHostedService<
    QuotationUpdatedConsumerWorker>();

// =============================
// DEPENDÊNCIAS COMPARTILHADAS
// =============================

builder.Services.RegisterDependencies(
    builder.Configuration);

// =============================
// JWT EXCLUSIVO DA API
// =============================

builder.Services.RegisterJwtConfiguration(
    builder.Configuration);

// =============================
// CONFIGURAÇÃO JWT
// =============================

var jwtOptions =
    builder.Configuration
        .GetSection(
            JwtOptions.SectionName)
        .Get<JwtOptions>()
    ?? throw new InvalidOperationException(
        "As configurações do JWT não foram encontradas.");

if (
    string.IsNullOrWhiteSpace(
        jwtOptions.SecretKey))
{
    throw new InvalidOperationException(
        "A chave secreta do JWT não foi configurada.");
}

var secretKey =
    Encoding.UTF8.GetBytes(
        jwtOptions.SecretKey);

builder.Services
    .AddAuthentication(
        JwtBearerDefaults
            .AuthenticationScheme)
    .AddJwtBearer(
        options =>
        {
            options.RequireHttpsMetadata =
                false;

            options.SaveToken =
                true;

            options.TokenValidationParameters =
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey =
                        true,

                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            secretKey),

                    ValidateIssuer =
                        true,

                    ValidIssuer =
                        jwtOptions.Issuer,

                    ValidateAudience =
                        true,

                    ValidAudience =
                        jwtOptions.Audience,

                    ValidateLifetime =
                        true,

                    ClockSkew =
                        TimeSpan.Zero
                };

            // Permite que o SignalR receba
            // o JWT pela query string.
            options.Events =
                new JwtBearerEvents
                {
                    OnMessageReceived =
                        context =>
                        {
                            var accessToken =
                                context
                                    .Request
                                    .Query[
                                        "access_token"];

                            var path =
                                context
                                    .HttpContext
                                    .Request
                                    .Path;

                            if (
                                !string.IsNullOrWhiteSpace(
                                    accessToken) &&
                                path.StartsWithSegments(
                                    "/hubs/quotations"))
                            {
                                context.Token =
                                    accessToken;
                            }

                            return Task.CompletedTask;
                        }
                };
        });

builder.Services.AddAuthorization();

// =============================
// HEALTH CHECK POSTGRESQL
// =============================

var connectionString =
    builder.Configuration
        .GetConnectionString(
            "DefaultConnection")
    ?? throw new InvalidOperationException(
        "A connection string 'DefaultConnection' não foi configurada.");

builder.Services
    .AddHealthChecks()
    .AddNpgSql(
        connectionString,
        name: "postgresql");

// =============================
// CORS ANGULAR + SIGNALR
// =============================

builder.Services.AddCors(
    options =>
    {
        options.AddPolicy(
            CorsPolicyName,
            policy =>
            {
                policy
                    .WithOrigins(
                        "http://localhost:4200")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
    });

var app =
    builder.Build();

// =============================
// LOGS E EXCEÇÕES
// =============================

app.UseMiddleware<
    RequestLoggingMiddleware>();

app.UseMiddleware<
    GlobalExceptionMiddleware>();

// =============================
// SWAGGER
// =============================

app.UseSwagger();

app.UseSwaggerUI();

app.UseHttpsRedirection();

// CORS precisa vir antes de
// autenticação/autorização.
app.UseCors(
    CorsPolicyName);

app.UseAuthentication();

app.UseAuthorization();

// =============================
// CONTROLLERS
// =============================

app.MapControllers();

// =============================
// SIGNALR
// =============================

app.MapHub<QuotationHub>(
    "/hubs/quotations");

// =============================
// HEALTH CHECK
// =============================

app.MapHealthChecks(
    "/health");

app.Run();

public partial class Program
{
}