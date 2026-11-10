using Serilog.Context;

namespace Admin.Observabilidad
{
    /// <summary>
    /// Asigna a cada petición un identificador de correlación (header X-Correlation-Id).
    /// Si llega desde el Gateway u otro microservicio se reutiliza; si no, se genera uno nuevo.
    /// Todos los logs de la petición llevan la propiedad CorrelationId, así se puede seguir en Seq
    /// una operación completa a través de varios microservicios.
    /// </summary>
    public class CorrelationIdMiddleware
    {
        public const string HeaderName = "X-Correlation-Id";
        public const string ItemKey = "CorrelationId";

        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = context.Request.Headers[HeaderName].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(correlationId))
                correlationId = Guid.NewGuid().ToString("N");

            context.Items[ItemKey] = correlationId;
            context.Response.OnStarting(() =>
            {
                context.Response.Headers[HeaderName] = correlationId;
                return Task.CompletedTask;
            });

            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await _next(context);
            }
        }
    }
}
