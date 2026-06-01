namespace MonitoringPlatform.Application.Models;

public class Result
{
    public bool IsSuccess { get; init; }
    public string Error { get; init; } = string.Empty;
    public List<ApiError> Errors { get; init; } = [];

    public static Result Success() => new() { IsSuccess = true };
    public static Result Failure(string error) => new() { Error = error, IsSuccess = false };
    public static Result Failure(List<ApiError> errors) => new() { IsSuccess = false, Errors = errors };

    protected Result() { }
}

public class Result<T> : Result
{
    public T? Value { get; init; }

    public static Result<T> Success(T value) => new() { Value = value, IsSuccess = true };
    public static new Result<T> Failure(string error) => new() { Error = error, IsSuccess = false };
    public static new Result<T> Failure(List<ApiError> errors) => new() { IsSuccess = false, Errors = errors };

    protected Result() { }
}
