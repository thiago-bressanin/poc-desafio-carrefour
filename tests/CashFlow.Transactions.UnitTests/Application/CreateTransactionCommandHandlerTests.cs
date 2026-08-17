using CashFlow.BuildingBlocks.Messaging;
using CashFlow.Transactions.Application.Commands.CreateTransaction;
using CashFlow.Transactions.Domain;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace CashFlow.Transactions.UnitTests.Application;

public class CreateTransactionCommandHandlerTests
{
    private readonly Mock<ITransactionRepository> _repositoryMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<IValidator<CreateTransactionCommand>> _validatorMock;
    private readonly Mock<ILogger<CreateTransactionCommandHandler>> _loggerMock;
    private readonly CreateTransactionCommandHandler _handler;

    public CreateTransactionCommandHandlerTests()
    {
        _repositoryMock = new Mock<ITransactionRepository>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _validatorMock = new Mock<IValidator<CreateTransactionCommand>>();
        _loggerMock = new Mock<ILogger<CreateTransactionCommandHandler>>();

        _handler = new CreateTransactionCommandHandler(
            _repositoryMock.Object,
            _publishEndpointMock.Object,
            _validatorMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_WithValidCreditCommand_ShouldPersistAndPublishEvent()
    {
        // Arrange
        var command = new CreateTransactionCommand(
            Guid.NewGuid(),
            "Credit",
            350.00m,
            DateTime.UtcNow.Date,
            "Recebimento de pagamento"
        );

        _validatorMock
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.MerchantId.Should().Be(command.MerchantId);
        result.Value.Type.Should().Be("Credit");
        result.Value.Amount.Should().Be(350.00m);

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _publishEndpointMock.Verify(p => p.Publish(It.Is<TransactionCreatedIntegrationEvent>(e =>
            e.MerchantId == command.MerchantId &&
            e.Amount == command.Amount &&
            e.Type == "Credit"
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ShouldReturnFailureResultWithoutSaving()
    {
        // Arrange
        var command = new CreateTransactionCommand(
            Guid.Empty,
            "InvalidType",
            -10m,
            DateTime.UtcNow,
            ""
        );

        var failures = new List<ValidationFailure>
        {
            new("MerchantId", "MerchantId é obrigatório."),
            new("Amount", "Valor deve ser maior que zero.")
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("VALIDATION_ERROR");

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Never);
        _publishEndpointMock.Verify(p => p.Publish(It.IsAny<TransactionCreatedIntegrationEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
