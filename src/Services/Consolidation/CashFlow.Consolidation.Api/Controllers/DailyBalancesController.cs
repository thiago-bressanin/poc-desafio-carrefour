using CashFlow.Consolidation.Application.DTOs;
using CashFlow.Consolidation.Application.Queries.GetDailyBalanceByDate;
using CashFlow.Consolidation.Application.Queries.GetDailyBalanceReport;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CashFlow.Consolidation.Api.Controllers;

[ApiController]
[Route("api/v1/daily-balances")]
[Produces("application/json")]
public class DailyBalancesController : ControllerBase
{
    private readonly IMediator _mediator;

    public DailyBalancesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Consulta o saldo consolidado de um dia específico para um lojista.
    /// </summary>
    /// <param name="date">Data da consulta (Formato: YYYY-MM-DD)</param>
    /// <param name="merchantId">Identificador único do lojista</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Saldo consolidado do dia</returns>
    [HttpGet("{date}")]
    [ProducesResponseType(typeof(DailyBalanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByDate(
        [FromRoute] DateOnly date,
        [FromQuery] Guid merchantId,
        CancellationToken cancellationToken)
    {
        var query = new GetDailyBalanceByDateQuery(merchantId, date);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Parâmetros Inválidos",
                Detail = result.Error,
                Extensions = { ["errorCode"] = result.ErrorCode }
            });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Emite o relatório de fluxo de caixa com saldos consolidados diários e saldo acumulado no período.
    /// </summary>
    /// <param name="merchantId">Identificador único do lojista</param>
    /// <param name="startDate">Data inicial do período (Formato: YYYY-MM-DD)</param>
    /// <param name="endDate">Data final do período (Formato: YYYY-MM-DD)</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Relatório consolidado com série diária e saldo acumulado</returns>
    [HttpGet]
    [ProducesResponseType(typeof(DailyBalanceReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetReport(
        [FromQuery] Guid merchantId,
        [FromQuery] DateOnly? startDate = null,
        [FromQuery] DateOnly? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var start = startDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var end = endDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var query = new GetDailyBalanceReportQuery(merchantId, start, end);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Parâmetros Inválidos",
                Detail = result.Error,
                Extensions = { ["errorCode"] = result.ErrorCode }
            });
        }

        return Ok(result.Value);
    }
}
