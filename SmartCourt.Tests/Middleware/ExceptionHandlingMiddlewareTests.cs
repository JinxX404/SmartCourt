using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Middleware;
using Xunit;

namespace SmartCourt.Tests.Middleware;

public sealed class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_UnexpectedException_ReturnsGenericResponseAndLogsExceptionWithTraceIdentifier()
    {
        var exception = new InvalidOperationException("Sensitive implementation details.");

        var result = await InvokeAsync(exception);

        Assert.Equal(StatusCodes.Status500InternalServerError, result.Context.Response.StatusCode);
        Assert.False(result.Response.Success);
        Assert.Equal("An internal server error occurred.", result.Response.Message);
        Assert.DoesNotContain(exception.Message, result.Body);
        Assert.DoesNotContain(exception.GetType().FullName!, result.Body);
        Assert.Same(exception, result.Logger.Exception);
        Assert.Contains(result.Context.TraceIdentifier, result.Logger.Message);
    }

    [Theory]
    [MemberData(nameof(ControlledExceptions))]
    public async Task InvokeAsync_ControlledException_PreservesStatusAndMessage(
        Exception exception,
        HttpStatusCode expectedStatusCode)
    {
        var result = await InvokeAsync(exception);

        Assert.Equal((int)expectedStatusCode, result.Context.Response.StatusCode);
        Assert.False(result.Response.Success);
        Assert.Equal(exception.Message, result.Response.Message);
    }

    [Fact]
    public async Task InvokeAsync_ValidationException_PreservesValidationErrors()
    {
        var exception = new ValidationException("Email", "Email is required.");

        var result = await InvokeAsync(exception);

        Assert.Equal(StatusCodes.Status400BadRequest, result.Context.Response.StatusCode);
        Assert.Equal(exception.Message, result.Response.Message);
        Assert.Equal(["Email: Email is required."], result.Response.Errors);
    }

    public static TheoryData<Exception, HttpStatusCode> ControlledExceptions => new()
    {
        { new AuthenticationException("Authentication failed."), HttpStatusCode.Unauthorized },
        { new ForbiddenAccessException("Forbidden access."), HttpStatusCode.Forbidden },
        { new NotFoundException("Resource was not found."), HttpStatusCode.NotFound },
        { new BusinessException("Business rule failed."), HttpStatusCode.BadRequest }
    };

    private static async Task<MiddlewareResult> InvokeAsync(Exception exception)
    {
        var context = new DefaultHttpContext
        {
            TraceIdentifier = "trace-identifier-123"
        };
        context.Response.Body = new MemoryStream();

        var logger = new TestLogger<ExceptionHandlingMiddleware>();
        var middleware = new ExceptionHandlingMiddleware(_ => throw exception, logger);

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();
        var response = JsonSerializer.Deserialize<ApiResponse<string>>(
            body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(response);

        return new MiddlewareResult(context, response, body, logger);
    }

    private sealed record MiddlewareResult(
        DefaultHttpContext Context,
        ApiResponse<string> Response,
        string Body,
        TestLogger<ExceptionHandlingMiddleware> Logger);

    private sealed class TestLogger<T> : ILogger<T>
    {
        public Exception? Exception { get; private set; }
        public string Message { get; private set; } = string.Empty;

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Exception = exception;
            Message = formatter(state, exception);
        }
    }

    private sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new();

        public void Dispose()
        {
        }
    }
}
