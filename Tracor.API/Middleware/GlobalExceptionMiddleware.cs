using System.Diagnostics;
using System.Text.Json;

namespace Tradecorp.API.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
        var statusCode = ex switch
        {
            InvalidOperationException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            BadHttpRequestException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        var title = statusCode switch
        {
            StatusCodes.Status400BadRequest => "Solicitud inválida",
            StatusCodes.Status401Unauthorized => "No autorizado",
            StatusCodes.Status404NotFound => "Recurso no encontrado",
            _ => "Error interno del servidor"
        };

        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        if (statusCode >= 500)
        {
            _logger.LogError(ex,
                "Unhandled exception. Method={Method} Path={Path} TraceId={TraceId}",
                context.Request.Method,
                context.Request.Path,
                traceId);
        }
        else
        {
            _logger.LogWarning(ex,
                "Handled business exception. Method={Method} Path={Path} TraceId={TraceId}",
                context.Request.Method,
                context.Request.Path,
                traceId);
        }

        if (context.Response.HasStarted)
        {
            _logger.LogWarning("Response already started for TraceId={TraceId}.", traceId);
            return;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var response = new
        {
            type = $"https://httpstatuses.com/{statusCode}",
            title,
            status = statusCode,
            detail = statusCode >= 500 ? "Ocurrió un error inesperado." : ex.Message,
            traceId
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}
