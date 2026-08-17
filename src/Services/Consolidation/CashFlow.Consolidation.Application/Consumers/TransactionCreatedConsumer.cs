using CashFlow.BuildingBlocks.Messaging;
using CashFlow.Consolidation.Application.Interfaces;
using CashFlow.Consolidation.Domain;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CashFlow.Consolidation.Application.Consumers;

public class TransactionCreatedConsumer : IConsumer<TransactionCreatedIntegrationEvent>
{
    private readonly IDailyBalanceRepository _dailyBalanceRepository;
    private readonly IProcessedTransactionRepository _processedTransactionRepository;
    private readonly IConsolidationCacheService _cacheService;
    private readonly ILogger<TransactionCreatedConsumer> _logger;

    public TransactionCreatedConsumer(
        IDailyBalanceRepository dailyBalanceRepository,
        IProcessedTransactionRepository processedTransactionRepository,
        IConsolidationCacheService cacheService,
        ILogger<TransactionCreatedConsumer> logger)
    {
        _dailyBalanceRepository = dailyBalanceRepository;
        _processedTransactionRepository = processedTransactionRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TransactionCreatedIntegrationEvent> context)
    {
        var message = context.Message;
        var transactionDate = DateOnly.FromDateTime(message.Date);

        _logger.LogInformation("Processando consolidação para a transação {TransactionId} do lojista {MerchantId} na data {Date}...",
            message.TransactionId, message.MerchantId, transactionDate);

        // 1. Verificação de Idempotência
        if (await _processedTransactionRepository.HasBeenProcessedAsync(message.TransactionId, context.CancellationToken))
        {
            _logger.LogInformation("Transação {TransactionId} já foi processada anteriormente. Ignorando duplicata (Idempotência).",
                message.TransactionId);
            return;
        }

        // 2. Busca ou cria o saldo consolidado do dia
        var balance = await _dailyBalanceRepository.GetByMerchantAndDateAsync(
            message.MerchantId, transactionDate, context.CancellationToken);

        if (balance is null)
        {
            balance = new DailyBalance(message.MerchantId, transactionDate);
            await _dailyBalanceRepository.AddAsync(balance, context.CancellationToken);
        }

        // 3. Aplica a movimentação financeira
        var isCredit = message.Type.StartsWith("Cred", StringComparison.OrdinalIgnoreCase);
        if (isCredit)
        {
            balance.ApplyCredit(message.Amount);
        }
        else
        {
            balance.ApplyDebit(message.Amount);
        }

        // 4. Marca como processado
        await _processedTransactionRepository.MarkAsProcessedAsync(message.TransactionId, context.CancellationToken);

        // 5. Salva no banco de dados
        await _dailyBalanceRepository.SaveChangesAsync(context.CancellationToken);

        // 6. Invalida cache para garantir coerência nas leituras subsequentes
        await _cacheService.InvalidateDailyBalanceAsync(message.MerchantId, transactionDate, context.CancellationToken);

        _logger.LogInformation("Consolidação concluída para a transação {TransactionId}. Novo Saldo do dia {Date}: Créditos={Credits}, Débitos={Debits}, Líquido={Net}.",
            message.TransactionId, transactionDate, balance.TotalCredits, balance.TotalDebits, balance.NetBalance);
    }
}
