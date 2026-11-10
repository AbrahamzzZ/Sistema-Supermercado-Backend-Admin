using Microsoft.AspNetCore.Diagnostics;
using Utilities.Shared;

namespace Admin.Observabilidad
{
    /// <summary>
    /// Captura cualquier excepción no controlada: la registra como Error (con el CorrelationId)
    /// y devuelve al cliente un 500 con el formato ApiResponse, sin exponer el stack trace.
    /// </summary>
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var correlationId = httpContext.Items[CorrelationIdMiddleware.ItemKey] as string ?? httpContext.TraceIdentifier;

            _logger.LogError(exception, "Error no controlado en {Metodo} {Ruta}. CorrelationId: {CorrelationId}",
                httpContext.Request.Method, httpContext.Request.Path, correlationId);

            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(new ApiResponse<object>
            {
                IsSuccess = false,
                Message = $"Ocurrió un error interno. Código de seguimiento: {correlationId}"
            }, cancellationToken);

            return true;
        }
    }
}
