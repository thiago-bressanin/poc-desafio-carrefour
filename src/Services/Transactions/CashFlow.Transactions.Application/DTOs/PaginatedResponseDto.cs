namespace CashFlow.Transactions.Application.DTOs;

public record PaginatedResponseDto<T>(
    IReadOnlyList<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages
);
