namespace CashFlow.BuildingBlocks.Domain;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public string? ErrorCode { get; }

    protected Result(bool isSuccess, string? error = null, string? errorCode = null)
    {
        if (isSuccess && error != null)
            throw new InvalidOperationException("Success result cannot have an error.");
        if (!isSuccess && error == null)
            throw new InvalidOperationException("Failure result must have an error.");

        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode;
    }

    public static Result Success() => new(true);
    public static Result Failure(string error, string? errorCode = null) => new(false, error, errorCode);
    public static Result<T> Success<T>(T value) => new(value, true);
    public static Result<T> Failure<T>(string error, string? errorCode = null) => new(default, false, error, errorCode);
}

public class Result<T> : Result
{
    private readonly T? _value;

    public T Value => IsSuccess 
        ? _value! 
        : throw new InvalidOperationException("Cannot access value of a failure result.");

    protected internal Result(T? value, bool isSuccess, string? error = null, string? errorCode = null)
        : base(isSuccess, error, errorCode)
    {
        _value = value;
    }
}
