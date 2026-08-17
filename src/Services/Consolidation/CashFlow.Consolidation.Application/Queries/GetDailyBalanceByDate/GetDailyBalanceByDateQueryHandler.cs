using CashFlow.BuildingBlocks.Domain;
using CashFlow.Consolidation.Application.DTOs;
using CashFlow.Consolidation.Application.Interfaces;
using CashFlow.Consolidation.Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CashFlow.Consolidation.Application.Queries.GetDailyBalanceByDate;

public class GetDailyBalanceByDateQueryHandler : IRequestHandler<GetDailyBalanceByDateQuery, Result<DailyBalanceDto>>
{
    private readonly IDailyBalanceRepository _repository;
    private readonly IConsolidationCacheService _cacheService;
    private readonly ILogger<GetDailyBalanceByDateQueryHandler> _logger;

    public GetDailyBalanceByDateQueryHandler(
        IDailyBalanceRepository repository,
        IConsolidationCacheService cacheService,
        ILogger<GetDailyBalanceByDateQueryHandler> logger)
    {
        _repository = repository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Result<DailyBalanceDto>> Handle(GetDailyBalanceByDateQuery request, CancellationToken cancellationToken)
    {
        if (request.MerchantId == Guid.Empty)
            return Result.Failure<DailyBalanceDto>("O identificador do lojista (MerchantId) é obrigatório.", "INVALID_MERCHANT_ID");

        // 1. Tenta recuperar do cache (Fast Path para suportar >= 50 req/s em picos)
        var cached = await _cacheService.GetDailyBalanceAsync(request.MerchantId, request.Date, cancellationToken);
        if (cached is not null)
        {
            return Result.Success(cached);
        }

        // 2. Busca do repositório
        var balance = await _repository.GetByMerchantAndDateAsync(request.MerchantId, request.Date, cancellationToken);

        DailyBalanceDto dto;
        if (balance is null)
        {
            // Retorna saldo zerado para dias sem movimentação
            dto = new DailyBalanceDto
            {
                MerchantId = request.MerchantId,
                Date = request.Date,
                TotalCredits = 0m,
                TotalDebits = 0m,
                NetBalance = 0m,
                CumulativeBalance = 0m,
                TotalTransactions = 0,
                LastUpdatedAt = DateTime.UtcNow
            };
        }
        else
        {
            dto = new DailyBalanceDto
            {
                MerchantId = balance.MerchantId,
                Date = balance.Date,
                TotalCredits = balance.TotalCredits,
                TotalDebits = balance.TotalDebits,
                NetBalance = balance.NetBalance,
                CumulativeBalance = balance.NetBalance,
                TotalTransactions = balance.TotalTransactions,
                LastUpdatedAt = balance.LastUpdatedAt
            };
        }

        // 3. Salva no cache com TTL padrão (5 minutos)
        await _cacheService.SetDailyBalanceAsync(request.MerchantId, request.Date, dto, TimeSpan.FromMinutes(5), cancellationToken);

        return Result.Success(dto);
    }
}
