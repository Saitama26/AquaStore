namespace frontend.ViewModels;

public sealed class ApiOperationResult
{
    public bool Success { get; }
    public string? Message { get; }
    public List<ApiError>? Errors { get; }

    public ApiOperationResult(bool success, string? message, List<ApiError>? errors)
    {
        Success = success;
        Message = message;
        Errors = errors;
    }

    public static ApiOperationResult Fail(string message) =>
        new(false, message, null);
}

public sealed class ApiOperationResult<T>
{
    public bool Success { get; }
    public T? Data { get; }
    public string? Message { get; }
    public List<ApiError>? Errors { get; }

    public ApiOperationResult(bool success, T? data, string? message, List<ApiError>? errors)
    {
        Success = success;
        Data = data;
        Message = message;
        Errors = errors;
    }

    public static ApiOperationResult<T> Fail(string message) =>
        new(false, default, message, null);
}

