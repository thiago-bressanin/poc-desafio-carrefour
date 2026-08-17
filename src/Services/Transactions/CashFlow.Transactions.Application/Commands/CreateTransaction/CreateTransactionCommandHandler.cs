using CashFlow.BuildingBlocks.Domain;
using CashFlow.BuildingBlocks.Messaging;
using CashFlow.Transactions.Application.DTOs;
using CashFlow.Transactions.Domain;
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CashFlow.Transactions.Application.Commands.CreateTransaction;

public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, Result<TransactionResponseDto>>
{
    private readonly ITransactionRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IValidator<CreateTransactionCommand> _validator;
    private readonly ILogger<CreateTransactionCommandHandler> _logger;

    public CreateTransactionCommandHandler(
        ITransactionRepository repository,
        IPublishEndpoint publishEndpoint,
        IValidator<CreateTransactionCommand> validator,
        ILogger<CreateTransactionCommandHandler> logger)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<TransactionResponseDto>> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            _logger.LogWarning("Falha de validação na criação de lançamento: {Errors}", errors);
            return Result.Failure<TransactionResponseDto>(errors, "VALIDATION_ERROR");
        }

        var isCredit = request.Type.StartsWith("Cred", StringComparison.OrdinalIgnoreCase);
        var type = isCredit ? TransactionType.Credit : TransactionType.Debit;

        try
        {
            var transaction = isCredit 
                ? Transaction.CreateCredit(request.MerchantId, request.Amount, request.Date, request.Description)
                : Transaction.CreateDebit(request.MerchantId, request.Amount, request.Date, request.Description);

            await _repository.AddAsync(transaction, cancellationToken);

            // Publica o evento de integração via Outbox do MassTransit.
            // O evento é persistido na mesma transação atômica do banco de dados (resiliência total).
            await _publishEndpoint.Publish(new TransactionCreatedIntegrationEvent
            {
                TransactionId = transaction.Id,
                MerchantId = transaction.MerchantId,
                Type = transaction.Type.ToString(),
                Amount = transaction.Amount,
                Date = transaction.Date,
                Description = transaction.Description,
                CreatedAt = transaction.CreatedAt
            }, cancellationToken);

            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Lançamento financeiro {TransactionId} do tipo {Type} no valor de {Amount} registrado com sucesso para o lojista {MerchantId}.",
                transaction.Id, transaction.Type, transaction.Amount, transaction.MerchantId);

            var response = new TransactionResponseDto
            {
                Id = transaction.Id,
                MerchantId = transaction.MerchantId,
                Type = transaction.Type.ToString(),
                Amount = transaction.Amount,
                Date = transaction.Date,
                Description = transaction.Description,
                CreatedAt = transaction.CreatedAt
            };

            return Result.Success(response);
        }
        catch (BusinessRuleException ex)
        {
            _logger.LogWarning(ex, "Violação de regra de negócio ao criar lançamento: {Message}", ex.Message);
            return Result.Failure<TransactionResponseDto>(ex.Message, ex.Code);
        }
    }
}
