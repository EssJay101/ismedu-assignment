using System.Diagnostics;
using Ismedu.Api.DTOs;
using Ismedu.Api.Exceptions;
using MySqlConnector;

namespace Ismedu.Api.Middleware;

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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var response = new ErrorResponse
        {
            TraceId = Activity.Current?.Id ?? context.TraceIdentifier
        };

        switch (ex)
        {
            case AppException appEx:
                response.Status = appEx.StatusCode;
                response.Message = appEx.Message;
                response.Errors = appEx.Errors;
                break;

            case MySqlException dbEx when dbEx.Number == 1062:
                response.Status = StatusCodes.Status409Conflict;
                response.Message = "A record with the same value already exists.";
                break;

            case MySqlException dbEx when dbEx.Number == 1451:
                response.Status = StatusCodes.Status409Conflict;
                response.Message = "This record is used by other records and cannot be deleted.";
                break;

            case MySqlException dbEx when dbEx.Number == 1452:
                response.Status = StatusCodes.Status400BadRequest;
                response.Message = "One of the selected related records does not exist.";
                break;

            default:
                _logger.LogError(ex, "Unhandled error");
                response.Status = StatusCodes.Status500InternalServerError;
                response.Message = "Something went wrong. Please try again later.";
                break;
        }

        context.Response.StatusCode = response.Status;
        await context.Response.WriteAsJsonAsync(response);
    }
}