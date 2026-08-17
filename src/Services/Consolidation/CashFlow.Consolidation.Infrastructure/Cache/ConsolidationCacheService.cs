using System.Text.Json;
using CashFlow.Consolidation.Application.DTOs;
using CashFlow.Consolidation.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace CashFlow.Consolidation.Infrastructure.Cache;

public class ConsolidationCacheService : IConsolidationCacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<ConsolidationCacheService> _logger;

    public ConsolidationCacheService(
        IDistributedCache cache,
        ILogger<ConsolidationCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    private static string GetCacheKey(Guid merchantId, DateOnly date) =>
        $"daily_balance:{merchantId}:{date:yyyyMMdd}";

    public async Task<DailyBalanceDto?> GetDailyBalanceAsync(
        Guid merchantId, 
        DateOnly date, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var key = GetCacheKey(merchantId, date);
            var cachedBytes = await _cache.GetAsync(key, cancellationToken);

            if (cachedBytes is null || cachedBytes.Length == 0)
                return null;

            return JsonSerializer.Deserialize<DailyBalanceDto>(cachedBytes);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao ler cache distribuído para o lojista {MerchantId} na data {Date}. Prosseguindo com consulta em banco.", merchantId, date);
            return null;
        }
    }

    public async Task SetDailyBalanceAsync(
        Guid merchantId, 
        DateOnly date, 
        DailyBalanceDto dto, 
        TimeSpan? expiration = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var key = GetCacheKey(merchantId, date);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(5)
            };

            var bytes = JsonSerializer.SerializeToUtf8Bytes(dto);
            await _cache.SetAsync(key, bytes, options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao gravar cache distribuído para o lojista {MerchantId} na data {Date}.", merchantId, date);
        }
    }

    public async Task InvalidateDailyBalanceAsync(
        Guid merchantId, 
        DateOnly date, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var key = GetCacheKey(merchantId, date);
            await _cache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao invalidar cache distribuído para a chave {Key}.", GetCacheKey(merchantId, date));
        }
    }
}
