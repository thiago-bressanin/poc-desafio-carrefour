using CashFlow.BuildingBlocks.Domain;
using CashFlow.Transactions.Application.DTOs;
using MediatR;

namespace CashFlow.Transactions.Application.Queries.GetTransactions;

public record GetTransactionsQuery(
    Guid MerchantId,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<Result<PaginatedResponseDto<TransactionResponseDto>>>;
