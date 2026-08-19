using SmartCourt.Common.Models;
using SmartCourt.Common.Exceptions;
using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SmartCourt.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An unhandled exception has occurred. TraceIdentifier: {TraceIdentifier}",
                context.TraceIdentifier);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var statusCode = (int)HttpStatusCode.InternalServerError;
        var message = "An internal server error occurred.";
        var errors = new System.Collections.Generic.List<string>();
        object? data = null;

        switch (exception)
        {
            case ValidationException e:
                statusCode = (int)HttpStatusCode.BadRequest;
                message = e.Message;
                errors = e.Errors.SelectMany(kv => kv.Value.Select(v => $"{kv.Key}: {v}")).ToList();
                break;
            case AuthenticationException e:
                statusCode = (int)HttpStatusCode.Unauthorized;
                message = e.Message;
                break;
            case InsufficientQuotaException e:
                statusCode = StatusCodes.Status429TooManyRequests;
                message = e.Message;
                data = new
                {
                    e.DailyLimitCredits,
                    e.ConsumedCredits,
                    e.RemainingCredits,
                    e.RequestedCredits,
                    e.NextResetAt
                };
                break;
            case BusinessException e:
                statusCode = (int)HttpStatusCode.BadRequest;
                message = e.Message;
                break;
            case NotFoundException e:
                statusCode = (int)HttpStatusCode.NotFound;
                message = e.Message;
                break;
            case ConflictException e:
                statusCode = (int)HttpStatusCode.Conflict;
                message = e.Message;
                break;
            case ForbiddenAccessException e:
                statusCode = (int)HttpStatusCode.Forbidden;
                message = string.IsNullOrWhiteSpace(e.Message) ? "Forbidden access." : e.Message;
                break;
            case PreconditionFailedException e:
                statusCode = StatusCodes.Status412PreconditionFailed;
                message = e.Message;
                break;
            case TooManyRequestsException e:
                statusCode = StatusCodes.Status429TooManyRequests;
                message = e.Message;
                break;
            case PayloadTooLargeException e:
                statusCode = StatusCodes.Status413PayloadTooLarge;
                message = e.Message;
                break;
        }

        context.Response.StatusCode = statusCode;

        object response;
        if (data != null)
        {
            var apiResponse = ApiResponse<object>.Fail(message, statusCode);
            apiResponse.Data = data;
            response = apiResponse;
        }
        else if (errors.Any())
        {
            var apiResponse = ApiResponse<string>.Fail(errors, statusCode);
            apiResponse.Message = message;
            response = apiResponse;
        }
        else
        {
            response = ApiResponse<string>.Fail(message, statusCode);
        }

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var jsonResponse = JsonSerializer.Serialize(response, options);

        return context.Response.WriteAsync(jsonResponse);
    }
}
