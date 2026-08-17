using CashFlow.Consolidation.Application.DTOs;
using CashFlow.Consolidation.Application.Interfaces;
using CashFlow.Consolidation.Application.Queries.GetDailyBalanceByDate;
using CashFlow.Consolidation.Domain;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CashFlow.Consolidation.UnitTests.Application;

public class GetDailyBalanceByDateQueryHandlerTests
{
    private readonly Mock<IDailyBalanceRepository> _repositoryMock;
    private readonly Mock<IConsolidationCacheService> _cacheServiceMock;
    private readonly Mock<ILogger<GetDailyBalanceByDateQueryHandler>> _loggerMock;
    private readonly GetDailyBalanceByDateQueryHandler _handler;

    public GetDailyBalanceByDateQueryHandlerTests()
    {
        _repositoryMock = new Mock<IDailyBalanceRepository>();
        _cacheServiceMock = new Mock<IConsolidationCacheService>();
        _loggerMock = new Mock<ILogger<GetDailyBalanceByDateQueryHandler>>();

        _handler = new GetDailyBalanceByDateQueryHandler(
            _repositoryMock.Object,
            _cacheServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenCachedValueExists_ShouldReturnFromCacheWithoutQueryingDb()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var date = new DateOnly(2026, 8, 17);
        var cachedDto = new DailyBalanceDto
        {
            MerchantId = merchantId,
            Date = date,
            TotalCredits = 500m,
            TotalDebits = 100m,
            NetBalance = 400m,
            CumulativeBalance = 400m,
            TotalTransactions = 2,
            LastUpdatedAt = DateTime.UtcNow
        };

        _cacheServiceMock
            .Setup(c => c.GetDailyBalanceAsync(merchantId, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedDto);

        var query = new GetDailyBalanceByDateQuery(merchantId, date);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(cachedDto);
        _repositoryMock.Verify(r => r.GetByMerchantAndDateAsync(It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenNotInCacheAndDbHasRecord_ShouldReturnAndPopulateCache()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var date = new DateOnly(2026, 8, 17);
        var dailyBalance = new DailyBalance(merchantId, date);
        dailyBalance.ApplyCredit(1000m);
        dailyBalance.ApplyDebit(300m);

        _cacheServiceMock
            .Setup(c => c.GetDailyBalanceAsync(merchantId, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DailyBalanceDto?)null);

        _repositoryMock
            .Setup(r => r.GetByMerchantAndDateAsync(merchantId, date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dailyBalance);

        var query = new GetDailyBalanceByDateQuery(merchantId, date);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCredits.Should().Be(1000m);
        result.Value.TotalDebits.Should().Be(300m);
        result.Value.NetBalance.Should().Be(700m);

        _cacheServiceMock.Verify(c => c.SetDailyBalanceAsync(
            merchantId, date, It.IsAny<DailyBalanceDto>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenMerchantIdIsEmpty_ShouldReturnFailureResult()
    {
        // Arrange
        var query = new GetDailyBalanceByDateQuery(Guid.Empty, new DateOnly(2026, 8, 17));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("INVALID_MERCHANT_ID");
    }
}
