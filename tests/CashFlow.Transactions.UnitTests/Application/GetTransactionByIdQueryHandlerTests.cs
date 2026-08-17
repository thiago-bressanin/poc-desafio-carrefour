using CashFlow.Transactions.Application.Queries.GetTransactionById;
using CashFlow.Transactions.Domain;
using FluentAssertions;
using Moq;
using Xunit;

namespace CashFlow.Transactions.UnitTests.Application;

public class GetTransactionByIdQueryHandlerTests
{
    private readonly Mock<ITransactionRepository> _repositoryMock;
    private readonly GetTransactionByIdQueryHandler _handler;

    public GetTransactionByIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<ITransactionRepository>();
        _handler = new GetTransactionByIdQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenTransactionExists_ShouldReturnSuccessResult()
    {
        // Arrange
        var merchantId = Guid.NewGuid();
        var transaction = Transaction.CreateCredit(merchantId, 250m, DateTime.UtcNow.Date, "Pagamento de conta");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(transaction.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        var query = new GetTransactionByIdQuery(transaction.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(transaction.Id);
        result.Value.MerchantId.Should().Be(merchantId);
        result.Value.Amount.Should().Be(250m);
        result.Value.Type.Should().Be("Credit");
    }

    [Fact]
    public async Task Handle_WhenTransactionDoesNotExist_ShouldReturnFailureResult()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _repositoryMock
            .Setup(r => r.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transaction?)null);

        var query = new GetTransactionByIdQuery(nonExistentId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("NOT_FOUND");
    }
}
