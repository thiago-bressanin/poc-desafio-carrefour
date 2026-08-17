using CashFlow.BuildingBlocks.Domain;
using CashFlow.Transactions.Application.DTOs;
using MediatR;

namespace CashFlow.Transactions.Application.Queries.GetTransactionById;

public record GetTransactionByIdQuery(Guid Id) : IRequest<Result<TransactionResponseDto>>;
