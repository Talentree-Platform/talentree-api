namespace Talentree.API.Models;

/// <summary>
/// Standard API response wrapper for all endpoints
/// Ensures consistent response format across the application
/// </summary>

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
    public DateTime Timestamp { get; set; }

    // Success response with data
    public static ApiResponse<T> SuccessResponse(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message,
            Timestamp = DateTime.UtcNow
        };
    }

    // Success response without data (for operations like Delete)
    public static ApiResponse<T> SuccessResponse(string message)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Timestamp = DateTime.UtcNow
        };
    }

    // Error response
    public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors,
            Timestamp = DateTime.UtcNow
        };
    }

    // Validation error response
    public static ApiResponse<T> ValidationErrorResponse(IDictionary<string, string[]> validationErrors)
    {
        var errors = validationErrors
            .SelectMany(e => e.Value.Select(v => $"{e.Key}: {v}"))
            .ToList();

        return new ApiResponse<T>
        {
            Success = false,
            Message = "Validation failed",
            Errors = errors,
            Timestamp = DateTime.UtcNow
        };
    }
}