using System.Net;
using System.Text.Json;
using Br.DollarQuotation.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Br.DollarQuotation.API.Middlewares;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(
                context,
                exception);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var problemDetails = CreateProblemDetails(
            context,
            exception);

        LogException(
            exception,
            problemDetails.Status);

        context.Response.StatusCode =
            problemDetails.Status
            ?? StatusCodes.Status500InternalServerError;

        context.Response.ContentType =
            "application/problem+json";

        var json = JsonSerializer.Serialize(
            problemDetails,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy =
                    JsonNamingPolicy.CamelCase
            });

        await context.Response.WriteAsync(json);
    }

    private static ProblemDetails CreateProblemDetails(
        HttpContext context,
        Exception exception)
    {
        var problemDetails = exception switch
        {
            EmailAlreadyRegisteredException =>
                CreateProblemDetails(
                    context,
                    StatusCodes.Status409Conflict,
                    "E-mail já cadastrado",
                    exception.Message),

            UserNotFoundException =>
                CreateProblemDetails(
                   context,
                   StatusCodes.Status404NotFound,
                   "Usuário não encontrado",
                   exception.Message),

            InvalidCredentialsException =>
                CreateProblemDetails(
                   context,
                   StatusCodes.Status401Unauthorized,
                   "Credenciais inválidas",
                    exception.Message),

            InactiveUserException =>
                CreateProblemDetails(
                    context,
                    StatusCodes.Status403Forbidden,
                    "Usuário inativo",
                    exception.Message),

            QuotationNotFoundException =>
                CreateProblemDetails(
                    context,
                    StatusCodes.Status404NotFound,
                    "Cotação não encontrada",
                    exception.Message),

            QuotationProviderException =>
                CreateProblemDetails(
                    context,
                    StatusCodes.Status502BadGateway,
                    "Erro no provedor de cotações",
                    exception.Message),

            DomainException =>
                CreateProblemDetails(
                    context,
                    StatusCodes.Status400BadRequest,
                    "Regra de negócio inválida",
                    exception.Message),

            ArgumentException =>
                CreateProblemDetails(
                    context,
                    StatusCodes.Status400BadRequest,
                    "Argumento inválido",
                    exception.Message),

            InvalidOperationException =>
                CreateProblemDetails(
                    context,
                    StatusCodes.Status400BadRequest,
                    "Operação inválida",
                    exception.Message),

            _ =>
                CreateProblemDetails(
                    context,
                    StatusCodes.Status500InternalServerError,
                    "Erro interno do servidor",
                    "Ocorreu um erro inesperado ao processar a requisição.")
        };

        problemDetails.Extensions["traceId"] =
            context.TraceIdentifier;

        return problemDetails;
    }

    private static ProblemDetails CreateProblemDetails(
        HttpContext context,
        int statusCode,
        string title,
        string detail)
    {
        return new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };
    }

    private void LogException(
        Exception exception,
        int? statusCode)
    {
        if (statusCode is >= 400 and < 500)
        {
            _logger.LogWarning(
                exception,
                "Erro tratado na requisição. StatusCode: {StatusCode}",
                statusCode);

            return;
        }

        _logger.LogError(
            exception,
            "Erro não tratado na requisição. StatusCode: {StatusCode}",
            statusCode);
    }
}