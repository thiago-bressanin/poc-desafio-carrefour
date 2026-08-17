using CashFlow.BuildingBlocks.Domain;
using CashFlow.Consolidation.Application.DTOs;
using CashFlow.Consolidation.Domain;
using MediatR;

namespace CashFlow.Consolidation.Application.Queries.GetDailyBalanceReport;

public class GetDailyBalanceReportQueryHandler : IRequestHandler<GetDailyBalanceReportQuery, Result<DailyBalanceReportDto>>
{
    private readonly IDailyBalanceRepository _repository;

    public GetDailyBalanceReportQueryHandler(IDailyBalanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<DailyBalanceReportDto>> Handle(GetDailyBalanceReportQuery request, CancellationToken cancellationToken)
    {
        if (request.MerchantId == Guid.Empty)
            return Result.Failure<DailyBalanceReportDto>("O identificador do lojista (MerchantId) é obrigatório.", "INVALID_MERCHANT_ID");

        if (request.StartDate > request.EndDate)
            return Result.Failure<DailyBalanceReportDto>("A data inicial não pode ser superior à data final.", "INVALID_DATE_RANGE");

        var balances = await _repository.GetByMerchantAndDateRangeAsync(
            request.MerchantId, request.StartDate, request.EndDate, cancellationToken);

        var balanceDict = balances.ToDictionary(b => b.Date);

        var dailyBalanceList = new List<DailyBalanceDto>();
        decimal runningCumulative = 0m;
        decimal totalPeriodCredits = 0m;
        decimal totalPeriodDebits = 0m;

        // Itera dia a dia para gerar a série temporal completa com saldo acumulado
        for (var date = request.StartDate; date <= request.EndDate; date = date.AddDays(1))
        {
            if (balanceDict.TryGetValue(date, out var balance))
            {
                runningCumulative += balance.NetBalance;
                totalPeriodCredits += balance.TotalCredits;
                totalPeriodDebits += balance.TotalDebits;

                dailyBalanceList.Add(new DailyBalanceDto
                {
                    MerchantId = request.MerchantId,
                    Date = date,
                    TotalCredits = balance.TotalCredits,
                    TotalDebits = balance.TotalDebits,
                    NetBalance = balance.NetBalance,
                    CumulativeBalance = runningCumulative,
                    TotalTransactions = balance.TotalTransactions,
                    LastUpdatedAt = balance.LastUpdatedAt
                });
            }
            else
            {
                dailyBalanceList.Add(new DailyBalanceDto
                {
                    MerchantId = request.MerchantId,
                    Date = date,
                    TotalCredits = 0m,
                    TotalDebits = 0m,
                    NetBalance = 0m,
                    CumulativeBalance = runningCumulative,
                    TotalTransactions = 0,
                    LastUpdatedAt = DateTime.UtcNow
                });
            }
        }

        var report = new DailyBalanceReportDto
        {
            MerchantId = request.MerchantId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            TotalPeriodCredits = Math.Round(totalPeriodCredits, 2),
            TotalPeriodDebits = Math.Round(totalPeriodDebits, 2),
            TotalPeriodNetBalance = Math.Round(totalPeriodCredits - totalPeriodDebits, 2),
            DailyBalances = dailyBalanceList
        };

        return Result.Success(report);
    }
}
