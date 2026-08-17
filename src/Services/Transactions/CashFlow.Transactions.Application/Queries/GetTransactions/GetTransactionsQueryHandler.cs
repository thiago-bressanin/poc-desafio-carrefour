using CashFlow.BuildingBlocks.Domain;
using CashFlow.Transactions.Application.DTOs;
using CashFlow.Transactions.Domain;
using MediatR;

namespace CashFlow.Transactions.Application.Queries.GetTransactions;

public class GetTransactionsQueryHandler : IRequestHandler<GetTransactionsQuery, Result<PaginatedResponseDto<TransactionResponseDto>>>
{
    private readonly ITransactionRepository _repository;

    public GetTransactionsQueryHandler(ITransactionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PaginatedResponseDto<TransactionResponseDto>>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        if (request.MerchantId == Guid.Empty)
            return Result.Failure<PaginatedResponseDto<TransactionResponseDto>>("O MerchantId é obrigatório para consultar lançamentos.", "INVALID_MERCHANT_ID");

        var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);

        var startDate = request.StartDate ?? DateTime.UtcNow.AddMonths(-1).Date;
        var endDate = request.EndDate ?? DateTime.UtcNow.Date;

        var skip = (pageNumber - 1) * pageSize;

        var totalCount = await _repository.CountByMerchantAndDateRangeAsync(
            request.MerchantId, startDate, endDate, cancellationToken);

        var transactions = await _repository.GetByMerchantAndDateRangeAsync(
            request.MerchantId, startDate, endDate, skip, pageSize, cancellationToken);

        var items = transactions.Select(t => new TransactionResponseDto
        {
            Id = t.Id,
            MerchantId = t.MerchantId,
            Type = t.Type.ToString(),
            Amount = t.Amount,
            Date = t.Date,
            Description = t.Description,
            CreatedAt = t.CreatedAt
        }).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var paginatedResult = new PaginatedResponseDto<TransactionResponseDto>(
            items,
            pageNumber,
            pageSize,
            totalCount,
            totalPages
        );

        return Result.Success(paginatedResult);
    }
}
