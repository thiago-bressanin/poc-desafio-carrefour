using System.Net;
using System.Text.Json;
using CashFlow.BuildingBlocks.Domain;
using Microsoft.AspNetCore.Mvc;

namespace CashFlow.Consolidation.Api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (BusinessRuleException ex)
        {
            _logger.LogWarning(ex, "Exceção de regra de negócio: {Message}", ex.Message);

            var problemDetails = new ProblemDetails
            {
                Status = (int)HttpStatusCode.UnprocessableEntity,
                Title = "Violação de Regra de Negócio",
                Detail = ex.Message,
                Instance = context.Request.Path,
                Extensions = { ["errorCode"] = ex.Code }
            };

            context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno não tratado no servidor de consolidação");

            var problemDetails = new ProblemDetails
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Title = "Erro Interno no Servidor",
                Detail = "Ocorreu um erro inesperado ao processar a consolidação. Por favor, tente novamente mais tarde.",
                Instance = context.Request.Path
            };

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
        }
    }
}
