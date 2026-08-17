using CashFlow.Transactions.Application.Commands.CreateTransaction;
using CashFlow.Transactions.Application.DTOs;
using CashFlow.Transactions.Application.Queries.GetTransactionById;
using CashFlow.Transactions.Application.Queries.GetTransactions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CashFlow.Transactions.Api.Controllers;

[ApiController]
[Route("api/v1/transactions")]
[Produces("application/json")]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransactionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Registra um novo lançamento financeiro (Crédito ou Débito).
    /// </summary>
    /// <param name="dto">Dados do lançamento financeiro</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Dados do lançamento criado com sucesso</returns>
    [HttpPost]
    [ProducesResponseType(typeof(TransactionResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTransactionRequestDto dto,
        CancellationToken cancellationToken)
    {
        var command = new CreateTransactionCommand(
            dto.MerchantId,
            dto.Type,
            dto.Amount,
            dto.Date,
            dto.Description
        );

        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            if (result.ErrorCode == "VALIDATION_ERROR")
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Requisição Inválida",
                    Detail = result.Error,
                    Extensions = { ["errorCode"] = result.ErrorCode }
                });
            }

            return UnprocessableEntity(new ProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "Regra de Negócio Violada",
                Detail = result.Error,
                Extensions = { ["errorCode"] = result.ErrorCode }
            });
        }

        return CreatedAtAction(
            nameof(GetById), 
            new { id = result.Value.Id }, 
            result.Value);
    }

    /// <summary>
    /// Obtém os detalhes de um lançamento específico por Id.
    /// </summary>
    /// <param name="id">Identificador único do lançamento</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Dados do lançamento financeiro</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TransactionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTransactionByIdQuery(id), cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Não Encontrado",
                Detail = result.Error,
                Extensions = { ["errorCode"] = result.ErrorCode }
            });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Consulta os lançamentos financeiros de um lojista com filtro por período e paginação.
    /// </summary>
    /// <param name="merchantId">Identificador do lojista</param>
    /// <param name="startDate">Data inicial de filtro (opcional)</param>
    /// <param name="endDate">Data final de filtro (opcional)</param>
    /// <param name="pageNumber">Número da página (padrão: 1)</param>
    /// <param name="pageSize">Tamanho da página (padrão: 20)</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Lista paginada de lançamentos</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponseDto<TransactionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid merchantId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTransactionsQuery(merchantId, startDate, endDate, pageNumber, pageSize);
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
