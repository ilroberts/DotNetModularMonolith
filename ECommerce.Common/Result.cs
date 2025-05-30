namespace ECommerce.Common;

public class Result<TSuccess, TError>
{
    public bool IsSuccess { get; }
    public TSuccess? Value { get; }
    public TError? Error { get; }

    private Result(bool isSuccess, TSuccess? value, TError? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<TSuccess, TError> Success(TSuccess value) => new(true, value, default);
    public static Result<TSuccess, TError> Failure(TError error) => new(false, default, error);
}

