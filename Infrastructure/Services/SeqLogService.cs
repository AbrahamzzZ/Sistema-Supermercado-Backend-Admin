using Domain.Model.Dto;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Utilities.Shared;

namespace Infrastructure.Services
{
    public class SeqLogService
    {
        public const string NombreHttpClient = "Seq";

        private const string FiltroBase = "(@Level = 'Error' or @Level = 'Warning') " +
            "and not (StartsWith(SourceContext, 'Microsoft') or StartsWith(SourceContext, 'System') or StartsWith(SourceContext, 'Serilog')) " +
            "and not (@Level = 'Warning' and StartsWith(SourceContext, 'Ocelot'))";

        private const int MaximoEventos = 5000;
        private static readonly TimeSpan ZonaEcuador = TimeSpan.FromHours(-5);

        private readonly HttpClient _client;
        private readonly ILogger<SeqLogService> _logger;

        public SeqLogService(IHttpClientFactory factory, ILogger<SeqLogService> logger)
        {
            _client = factory.CreateClient(NombreHttpClient);
            _logger = logger;
        }

        public async Task<ApiResponse<Paginacion<LogSistema>>> ListarPaginacionAsync(
            int pageNumber, int pageSize, string? filtro, DateOnly? desde, DateOnly? hasta)
        {
            pageNumber = Math.Max(pageNumber, 1);
            pageSize = Math.Clamp(pageSize, 1, 500);

            var filtroSeq = ConstruirFiltro(filtro);
            var rango = RangoUtc(desde, hasta);

            try
            {
                var total = await ContarAsync(filtroSeq, rango);
                var hastaEvento = Math.Min(pageNumber * pageSize, MaximoEventos);
                var eventos = await ObtenerEventosAsync(filtroSeq, rango, hastaEvento);

                var items = eventos
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(Mapear)
                    .ToList();

                return new ApiResponse<Paginacion<LogSistema>>
                {
                    IsSuccess = true,
                    Message = items.Count == 0 ? Mensajes.MESSAGE_QUERY_EMPTY : Mensajes.MESSAGE_QUERY,
                    Data = new Paginacion<LogSistema>
                    {
                        Items = items,
                        TotalCount = Math.Min(total, MaximoEventos),
                        PageNumber = pageNumber,
                        PageSize = pageSize
                    }
                };
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                _logger.LogError(ex, "No se pudieron consultar los logs en Seq");
                return new ApiResponse<Paginacion<LogSistema>> { IsSuccess = false, Message = Mensajes.MESSAGE_LOGS_NO_DISPONIBLES };
            }
        }

        public async Task<ApiResponse<LogSistema>> ObtenerAsync(string id)
        {
            try
            {
                var response = await _client.GetAsync($"/api/events/{Uri.EscapeDataString(id)}?render=true");
                if (!response.IsSuccessStatusCode)
                    return new ApiResponse<LogSistema> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_NOT_FOUND };

                var evento = await response.Content.ReadFromJsonAsync<EventoSeq>();
                return evento is null
                    ? new ApiResponse<LogSistema> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_NOT_FOUND }
                    : new ApiResponse<LogSistema> { IsSuccess = true, Message = Mensajes.MESSAGE_QUERY, Data = Mapear(evento) };
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                _logger.LogError(ex, "No se pudo consultar el log {IdLog} en Seq", id);
                return new ApiResponse<LogSistema> { IsSuccess = false, Message = Mensajes.MESSAGE_LOGS_NO_DISPONIBLES };
            }
        }

        private async Task<int> ContarAsync(string filtro, (string? Desde, string? Hasta) rango)
        {
            var url = $"/api/data?q={Uri.EscapeDataString($"select count(*) as Total from stream where {filtro}")}"
                      + (rango.Desde is null ? "" : $"&rangeStartUtc={rango.Desde}")
                      + (rango.Hasta is null ? "" : $"&rangeEndUtc={rango.Hasta}");

            var resultado = await _client.GetFromJsonAsync<ResultadoConsultaSeq>(url);
            var valor = resultado?.Rows?.FirstOrDefault()?.FirstOrDefault();
            return valor is JsonElement { ValueKind: JsonValueKind.Number } numero ? numero.GetInt32() : 0;
        }

        private async Task<List<EventoSeq>> ObtenerEventosAsync(string filtro, (string? Desde, string? Hasta) rango, int cantidad)
        {
            var url = $"/api/events?render=true&count={cantidad}&filter={Uri.EscapeDataString(filtro)}"
                      + (rango.Desde is null ? "" : $"&fromDateUtc={rango.Desde}")
                      + (rango.Hasta is null ? "" : $"&toDateUtc={rango.Hasta}");

            return await _client.GetFromJsonAsync<List<EventoSeq>>(url) ?? [];
        }
        private static string ConstruirFiltro(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return FiltroBase;

            var t = texto.Trim().Replace("'", "''"); 
            return $"({FiltroBase}) and (Contains(@Message, '{t}') ci or Contains(Coalesce(CorrelationId, ''), '{t}') ci " +
                   $"or Contains(Coalesce(RequestPath, ''), '{t}') ci or Contains(Coalesce(Servicio, ''), '{t}') ci)";
        }

        private static (string? Desde, string? Hasta) RangoUtc(DateOnly? desde, DateOnly? hasta)
        {
            static string Utc(DateOnly dia) =>
                new DateTimeOffset(dia.ToDateTime(TimeOnly.MinValue), ZonaEcuador).UtcDateTime
                    .ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);

            return (desde is null ? null : Utc(desde.Value), hasta is null ? null : Utc(hasta.Value.AddDays(1)));
        }

        private static LogSistema Mapear(EventoSeq e)
        {
            var p = (e.Properties ?? []).ToDictionary(x => x.Name, x => x.Value);

            string? Texto(string nombre) =>
                p.TryGetValue(nombre, out var v) && v.ValueKind is not (JsonValueKind.Null or JsonValueKind.Undefined)
                    ? (v.ValueKind == JsonValueKind.String ? v.GetString() : v.ToString())
                    : null;

            return new LogSistema
            {
                Id_Log = e.Id ?? "",
                Codigo = Texto("CorrelationId"),
                Mensaje = e.RenderedMessage,
                Detalle = e.Exception,
                Id_Usuario = int.TryParse(Texto("IdUsuario"), out var idUsuario) ? idUsuario : null,
                Fecha = e.Timestamp,
                Endpoint = Texto("RequestPath"),
                Metodo = Texto("RequestMethod"),
                Nivel = e.Level,
                Servicio = Texto("Servicio")
            };
        }

        private sealed class EventoSeq
        {
            public string? Id { get; set; }
            public DateTimeOffset Timestamp { get; set; }
            public string? Level { get; set; }
            public string? RenderedMessage { get; set; }
            public string? Exception { get; set; }
            public List<PropiedadSeq>? Properties { get; set; }
        }

        private sealed class PropiedadSeq
        {
            public string Name { get; set; } = "";
            public JsonElement Value { get; set; }
        }

        private sealed class ResultadoConsultaSeq
        {
            public List<List<JsonElement>>? Rows { get; set; }
        }
    }
}
