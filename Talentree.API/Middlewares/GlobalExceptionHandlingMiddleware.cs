
using System.Net;
using System.Text.Json;
using Talentree.API.Models;
using Talentree.Core.Exceptions;

namespace Guidy.API.Middleware;

/// <summary>
/// Global exception handling middleware
/// Catches all unhandled exceptions and returns consistent error responses
/// </summary>
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger)
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

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Log the exception
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        // Determine status code and response based on exception type
        var (statusCode, response) = exception switch
        {
            BadRequestException badRequestEx => (
                HttpStatusCode.BadRequest,
                ApiResponse<object>.ErrorResponse(badRequestEx.Message)
            ),

            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                ApiResponse<object>.ErrorResponse(notFoundEx.Message)
            ),

            UnauthorizedException unauthorizedEx => (
                HttpStatusCode.Unauthorized,
                ApiResponse<object>.ErrorResponse(unauthorizedEx.Message)
            ),

            ForbiddenException forbiddenEx => (
                HttpStatusCode.Forbidden,
                ApiResponse<object>.ErrorResponse(forbiddenEx.Message)
            ),

            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                ApiResponse<object>.ValidationErrorResponse(validationEx.Errors)
            ),

            _ => (
                HttpStatusCode.InternalServerError,
                ApiResponse<object>.ErrorResponse(
                    "An internal server error occurred. Please try again later.",
                    new List<string> { exception.Message }
                )
            )
        };

        // Set response properties
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        // Serialize and write response
        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}
