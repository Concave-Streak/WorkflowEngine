namespace WorkflowEngine.DTOs;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> ValidationErrors { get; set; } = new();

    public static ApiResponse<T> SuccessResult(T data) => new()
    {
        Success = true,
        Data = data
    };

    public static ApiResponse<T> ErrorResult(string message) => new()
    {
        Success = false,
        ErrorMessage = message
    };

    public static ApiResponse<T> ValidationErrorResult(List<string> errors) => new()
    {
        Success = false,
        ValidationErrors = errors
    };
}
