using QueueAndPray.Application.Common.Exceptions;
using QueueAndPray.Contracts.Common;
using System.Net;
using System.Text.Json;

namespace QueueAndPray.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (AppException ex)
        {
            _logger.LogWarning(ex, "Not found error occurred");

            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";

            var error = new ApiError
            {
                Code = ex.Code,
                Message = ex.Message
            };

            var json = JsonSerializer.Serialize(error);

            await context.Response.WriteAsync(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");

            context.Response.StatusCode =
                (int)HttpStatusCode.InternalServerError;

            context.Response.ContentType = "application/json";

            var error = new ApiError
            {
                Code = "internal_server_error",
                Message = "Unexpected error occurred."
            };

            var json = JsonSerializer.Serialize(error);

            await context.Response.WriteAsync(json);
        }
    }
}