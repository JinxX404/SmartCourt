using System.Collections.Generic;

namespace SmartCourt.Common.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
    public int StatusCode { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null)
        => new() { Success = true, Data = data, StatusCode = 200, Message = message };

    public static ApiResponse<T> Created(T data)
        => new() { Success = true, Data = data, StatusCode = 201 };

    public static ApiResponse<T> Fail(string message, int statusCode = 400)
        => new() { Success = false, Message = message, StatusCode = statusCode };

    public static ApiResponse<T> Fail(List<string> errors, int statusCode = 400)
        => new() { Success = false, Errors = errors, StatusCode = statusCode };
}
