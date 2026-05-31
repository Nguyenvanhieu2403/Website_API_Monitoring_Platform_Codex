namespace MonitoringPlatform.Application.Models;

public class Result<T>
{
    public T? Value { get; set; }
    public bool IsSuccess { get; set; }
    public string Error { get; set; } = string.Empty;

    public static Result<T> Success(T value) => new() { Value = value, IsSuccess = true };
    public static Result<T> Failure(string error) => new() { Error = error, IsSuccess = false };
}
