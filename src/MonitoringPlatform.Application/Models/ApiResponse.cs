namespace MonitoringPlatform.Application.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }

    public int StatusCode { get; set; }

    public string Message { get; set; } = string.Empty;

    public T? Data { get; set; }

    public List<ApiError> Errors { get; set; } = [];

    public string TraceId { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ApiError
{
    public string Field { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}
