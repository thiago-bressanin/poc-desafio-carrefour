using CashFlow.BuildingBlocks.Messaging;
using CashFlow.Consolidation.Application.Consumers;
using CashFlow.Consolidation.Application.Interfaces;
using CashFlow.Consolidation.Domain;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CashFlow.Consolidation.UnitTests.Application;

public class TransactionCreatedConsumerTests
{
    private readonly Mock<IDailyBalanceRepository> _dailyBalanceRepoMock;
    private readonly Mock<IProcessedTransactionRepository> _processedRepoMock;
    private readonly Mock<IConsolidationCacheService> _cacheMock;
    private readonly Mock<ILogger<TransactionCreatedConsumer>> _loggerMock;
    private readonly TransactionCreatedConsumer _consumer;

    public TransactionCreatedConsumerTests()
    {
        _dailyBalanceRepoMock = new Mock<IDailyBalanceRepository>();
        _processedRepoMock = new Mock<IProcessedTransactionRepository>();
        _cacheMock = new Mock<IConsolidationCacheService>();
        _loggerMock = new Mock<ILogger<TransactionCreatedConsumer>>();

        _consumer = new TransactionCreatedConsumer(
            _dailyBalanceRepoMock.Object,
            _processedRepoMock.Object,
            _cacheMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Consume_WhenTransactionNotYetProcessed_ShouldConsolidateAndInvalidateCache()
    {
        // Arrange
        var txId = Guid.NewGuid();
        var merchantId = Guid.NewGuid();
        var date = DateTime.UtcNow.Date;

        var message = new TransactionCreatedIntegrationEvent
        {
            TransactionId = txId,
            MerchantId = merchantId,
            Type = "Credit",
            Amount = 200.00m,
            Date = date,
            Description = "Venda",
            CreatedAt = DateTime.UtcNow
        };

        var contextMock = new Mock<ConsumeContext<TransactionCreatedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(message);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        _processedRepoMock
            .Setup(p => p.HasBeenProcessedAsync(txId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        DailyBalance? existingBalance = null;
        _dailyBalanceRepoMock
            .Setup(r => r.GetByMerchantAndDateAsync(merchantId, DateOnly.FromDateTime(date), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBalance);

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        _dailyBalanceRepoMock.Verify(r => r.AddAsync(It.Is<DailyBalance>(b => 
            b.MerchantId == merchantId && b.TotalCredits == 200.00m), It.IsAny<CancellationToken>()), Times.Once);
        _processedRepoMock.Verify(p => p.MarkAsProcessedAsync(txId, It.IsAny<CancellationToken>()), Times.Once);
        _dailyBalanceRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(c => c.InvalidateDailyBalanceAsync(merchantId, DateOnly.FromDateTime(date), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_WhenTransactionAlreadyProcessed_ShouldIgnoreToGuaranteeIdempotency()
    {
        // Arrange
        var txId = Guid.NewGuid();
        var message = new TransactionCreatedIntegrationEvent
        {
            TransactionId = txId,
            MerchantId = Guid.NewGuid(),
            Type = "Debit",
            Amount = 50.00m,
            Date = DateTime.UtcNow,
            Description = "Despesa",
            CreatedAt = DateTime.UtcNow
        };

        var contextMock = new Mock<ConsumeContext<TransactionCreatedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(message);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        _processedRepoMock
            .Setup(p => p.HasBeenProcessedAsync(txId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true); // Already processed!

        // Act
        await _consumer.Consume(contextMock.Object);

        // Assert
        _dailyBalanceRepoMock.Verify(r => r.AddAsync(It.IsAny<DailyBalance>(), It.IsAny<CancellationToken>()), Times.Never);
        _dailyBalanceRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _cacheMock.Verify(c => c.InvalidateDailyBalanceAsync(It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
