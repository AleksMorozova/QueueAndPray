namespace QueueAndPray.Contracts.Common;

public class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public ApiError? Error { get; init; }

    public static ApiResponse<T> Ok(T data)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data
        };
    }

    public static ApiResponse<T> Fail(ApiError error)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Error = error
        };
    }
}
