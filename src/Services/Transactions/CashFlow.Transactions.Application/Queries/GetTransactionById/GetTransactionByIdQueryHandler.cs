using CashFlow.BuildingBlocks.Domain;
using CashFlow.Transactions.Application.DTOs;
using CashFlow.Transactions.Domain;
using MediatR;

namespace CashFlow.Transactions.Application.Queries.GetTransactionById;

public class GetTransactionByIdQueryHandler : IRequestHandler<GetTransactionByIdQuery, Result<TransactionResponseDto>>
{
    private readonly ITransactionRepository _repository;

    public GetTransactionByIdQueryHandler(ITransactionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<TransactionResponseDto>> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        var transaction = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (transaction is null)
        {
            return Result.Failure<TransactionResponseDto>($"Lançamento com Id '{request.Id}' não foi encontrado.", "NOT_FOUND");
        }

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
}
