using CashFlow.BuildingBlocks.Domain;
using CashFlow.Transactions.Application.DTOs;
using MediatR;

namespace CashFlow.Transactions.Application.Commands.CreateTransaction;

public record CreateTransactionCommand(
    Guid MerchantId,
    string Type,
    decimal Amount,
    DateTime Date,
    string Description
) : IRequest<Result<TransactionResponseDto>>;
