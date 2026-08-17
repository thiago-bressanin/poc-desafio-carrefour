using CashFlow.Consolidation.Application.Queries.GetDailyBalanceReport;
using CashFlow.Consolidation.Domain;
using FluentAssertions;
using Moq;
using Xunit;

namespace CashFlow.Consolidation.UnitTests.Application;

public class GetDailyBalanceReportQueryHandlerTests
{
    [Fact]
    public async Task Handle_WithValidDateRange_ShouldCalculateDailyAndCumulativeBalances()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var day1 = new DateOnly(2026, 8, 1);
        var day2 = new DateOnly(2026, 8, 2);
        var day3 = new DateOnly(2026, 8, 3);

        var balanceDay1 = new DailyBalance(merchantId, day1);
        balanceDay1.ApplyCredit(1000m);
        balanceDay1.ApplyDebit(200m); // Net = +800

        var balanceDay3 = new DailyBalance(merchantId, day3);
        balanceDay3.ApplyCredit(500m);
        balanceDay3.ApplyDebit(100m); // Net = +400
        // day2 has no transactions (Net = 0)

        var repositoryMock = new Mock<IDailyBalanceRepository>();
        repositoryMock
            .Setup(r => r.GetByMerchantAndDateRangeAsync(merchantId, day1, day3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DailyBalance> { balanceDay1, balanceDay3 });

        var handler = new GetDailyBalanceReportQueryHandler(repositoryMock.Object);
        var query = new GetDailyBalanceReportQuery(merchantId, day1, day3);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var report = result.Value;
        report.Should().NotBeNull();
        report.TotalPeriodCredits.Should().Be(1500m);
        report.TotalPeriodDebits.Should().Be(300m);
        report.TotalPeriodNetBalance.Should().Be(1200m);

        report.DailyBalances.Should().HaveCount(3);

        // Day 1: Net = 800, Cumulative = 800
        report.DailyBalances[0].Date.Should().Be(day1);
        report.DailyBalances[0].NetBalance.Should().Be(800m);
        report.DailyBalances[0].CumulativeBalance.Should().Be(800m);

        // Day 2 (empty day): Net = 0, Cumulative = 800
        report.DailyBalances[1].Date.Should().Be(day2);
        report.DailyBalances[1].NetBalance.Should().Be(0m);
        report.DailyBalances[1].CumulativeBalance.Should().Be(800m);

        // Day 3: Net = 400, Cumulative = 1200
        report.DailyBalances[2].Date.Should().Be(day3);
        report.DailyBalances[2].NetBalance.Should().Be(400m);
        report.DailyBalances[2].CumulativeBalance.Should().Be(1200m);
    }
}
