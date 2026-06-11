using Backend_Proyecto.Models.DTOs;
using System.Net;
using System.Text.Json;

namespace Backend_Proyecto.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Solicitud invalida");
            await EscribirErrorAsync(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning(ex, "Recurso no encontrado");
            await EscribirErrorAsync(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Acceso no autorizado");
            await EscribirErrorAsync(context, HttpStatusCode.Unauthorized, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error interno del servidor");
            await EscribirErrorAsync(context, HttpStatusCode.InternalServerError, "Ocurrio un error interno. Intente nuevamente.");
        }
    }

    private static async Task EscribirErrorAsync(HttpContext context, HttpStatusCode status, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;

        var payload = new ApiErrorResponse { Message = message };
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
