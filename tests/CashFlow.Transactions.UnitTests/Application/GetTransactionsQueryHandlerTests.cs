using CashFlow.Transactions.Application.Queries.GetTransactions;
using CashFlow.Transactions.Domain;
using FluentAssertions;
using Moq;
using Xunit;

namespace CashFlow.Transactions.UnitTests.Application;

public class GetTransactionsQueryHandlerTests
{
    private readonly Mock<ITransactionRepository> _repositoryMock;
    private readonly GetTransactionsQueryHandler _handler;

    public GetTransactionsQueryHandlerTests()
    {
        _repositoryMock = new Mock<ITransactionRepository>();
        _handler = new GetTransactionsQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenMerchantIdIsEmpty_ShouldReturnFailureResult()
    {
        // Arrange
        var query = new GetTransactionsQuery(Guid.Empty);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("INVALID_MERCHANT_ID");
    }

    [Fact]
    public async Task Handle_WithValidParameters_ShouldReturnPaginatedTransactions()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var date = DateTime.UtcNow.Date;
        var t1 = Transaction.CreateCredit(merchantId, 100m, date, "Item 1");
        var t2 = Transaction.CreateDebit(merchantId, 50m, date, "Item 2");

        var transactions = new List<Transaction> { t1, t2 };

        _repositoryMock
            .Setup(r => r.CountByMerchantAndDateRangeAsync(merchantId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        _repositoryMock
            .Setup(r => r.GetByMerchantAndDateRangeAsync(merchantId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), 0, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactions);

        var query = new GetTransactionsQuery(merchantId, date.AddDays(-5), date, 1, 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(20);
        result.Value.TotalPages.Should().Be(1);
    }
}
