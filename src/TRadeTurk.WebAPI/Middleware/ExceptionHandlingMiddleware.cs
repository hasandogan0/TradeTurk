using System.Net;
using System.Text.Json;
using FluentValidation;

namespace TRadeTurk.WebAPI.Middleware;

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
            _logger.LogError(ex, "An unhandled exception has occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var statusCode = (int)HttpStatusCode.InternalServerError;
        string message = "Sunucu tarafında bir hata oluştu.";
        object? errors = null;

        switch (exception)
        {
            case ValidationException validationException:
                statusCode = (int)HttpStatusCode.BadRequest;
                message = "Validasyon hatası oluştu.";
                errors = validationException.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                break;
            case InvalidOperationException invalidOperationException:
                statusCode = (int)HttpStatusCode.BadRequest;
                message = invalidOperationException.Message;
                break;
            // Diğer özel exception tipleri buraya eklenebilir
        }

        context.Response.StatusCode = statusCode;

        var response = new
        {
            StatusCode = statusCode,
            Message = message,
            Errors = errors,
            Timestamp = DateTime.UtcNow
        };

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}
