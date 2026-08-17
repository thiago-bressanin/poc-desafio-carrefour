using CashFlow.BuildingBlocks.Domain;
using CashFlow.Consolidation.Application.DTOs;
using MediatR;

namespace CashFlow.Consolidation.Application.Queries.GetDailyBalanceByDate;

public record GetDailyBalanceByDateQuery(
    Guid MerchantId,
    DateOnly Date
) : IRequest<Result<DailyBalanceDto>>;
