using CashFlow.BuildingBlocks.Domain;
using CashFlow.Consolidation.Application.DTOs;
using MediatR;

namespace CashFlow.Consolidation.Application.Queries.GetDailyBalanceReport;

public record GetDailyBalanceReportQuery(
    Guid MerchantId,
    DateOnly StartDate,
    DateOnly EndDate
) : IRequest<Result<DailyBalanceReportDto>>;
