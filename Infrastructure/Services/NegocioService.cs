using Domain.Model;
using Domain.Model.Dto.Negocio;
using FluentValidation;
using Infrastructure.Repository;
using Infrastructure.Repository.InterfacesServices;
using Microsoft.Extensions.Logging;
using Utilities.IA;
using Utilities.Shared;

namespace Infrastructure.Services
{
    public class NegocioService : INegocioService
    {
        private readonly NegocioRepository _negocioRepository;
        private readonly IValidator<Negocio> _validator;
        private readonly OllamaClient _ollama;
        private readonly MicroserviciosApiClient _microservicios;
        private readonly ILogger<NegocioService> _logger;

        public NegocioService(NegocioRepository negocioRepository, IValidator<Negocio> validator, OllamaClient ollama,
            MicroserviciosApiClient microservicios, ILogger<NegocioService> logger)
        {
            _logger = logger;
            _negocioRepository = negocioRepository;
            _validator = validator;
            _ollama = ollama;
            _microservicios = microservicios;
        }

        public async Task<ApiResponse<Negocio>> ObtenerNegocioAsync(int idNegocio)
        {
            var negocio = await _negocioRepository.ObtenerNegocioAsync(idNegocio);
            if (negocio == null)
                return new ApiResponse<Negocio> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_NOT_FOUND };

            return new ApiResponse<Negocio> { IsSuccess = true, Message = Mensajes.MESSAGE_QUERY, Data = negocio };
        }

        public async Task<ApiResponse<object>> EditarNegocioAsync(Negocio negocio)
        {
            if (negocio == null)
                return ReglaIncumplida<object>(Mensajes.MESSAGE_NULL, "EditarNegocioAsync", negocio?.Id_Negocio);

            var validationResult = await _validator.ValidateAsync(negocio);
            if (!validationResult.IsValid)
                return ReglaIncumplida<object>(string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage)), "EditarNegocioAsync", negocio?.Id_Negocio);

            var negocioExistente = await _negocioRepository.ObtenerNegocioAsync(negocio.Id_Negocio);
            if (negocioExistente == null)
                return ReglaIncumplida<object>(Mensajes.MESSAGE_QUERY_NOT_FOUND, "EditarNegocioAsync", negocio?.Id_Negocio);

            var result = await _negocioRepository.EditarNegocioAsync(negocio);
            if (result > 0)
                return new ApiResponse<object> { IsSuccess = true, Message = Mensajes.MESSAGE_UPDATE };

            return ReglaIncumplida<object>(Mensajes.MESSAGE_UPDATE_FAILLED, "EditarNegocioAsync", negocio?.Id_Negocio);
        }

        public Task<ApiResponse<List<ProductoMasComprado>>> ObtenerProductoMasComprado()
            => ReporteAsync("productos-comprados", _negocioRepository.ObtenerNombresProductosAsync, "Producto", (nombre, total) => new ProductoMasComprado { Nombre_Producto = nombre, Cantidad_Comprada = total });

        public Task<ApiResponse<List<ProductoMasVendido>>> ObtenerProductoMasVendido()
            => ReporteAsync("productos-vendidos", _negocioRepository.ObtenerNombresProductosAsync, "Producto", (nombre, total) => new ProductoMasVendido { Nombre_Producto = nombre, Cantidad_Vendida = total });

        public Task<ApiResponse<List<TopCliente>>> ObtenerTopClientes()
            => ReporteAsync("top-clientes", _negocioRepository.ObtenerNombresClientesAsync, "Cliente", (nombre, total) => new TopCliente { Nombre_Completo = nombre, Compras_Totales = total });

        public Task<ApiResponse<List<TopProveedor>>> ObtenerTopProveedores()
            => ReporteAsync("top-proveedores", _negocioRepository.ObtenerNombresProveedoresAsync, "Proveedor", (nombre, total) => new TopProveedor { Nombre_Completo = nombre, Compras_Totales = total });

        public Task<ApiResponse<List<ViajesTransportista>>> ObtenerViajesTransportista()
            => ReporteAsync("viajes-transportista", _negocioRepository.ObtenerNombresTransportistasAsync, "Transportista", (nombre, total) => new ViajesTransportista { Nombre_Completo = nombre, Viajes_Realizados = total });

        public Task<ApiResponse<List<EmpleadoProductivo>>> ObtenerEmpleadosProductivos()
            => ReporteAsync("ventas-empleado", NombresEmpleadosAsync, "Empleado", (nombre, total) => new EmpleadoProductivo { Nombre_Completo = nombre, Ventas_Empleado = total });

        private async Task<ApiResponse<List<T>>> ReporteAsync<T>(
            string reporte,
            Func<IEnumerable<int>, Task<Dictionary<int, string>>> obtenerNombres,
            string etiqueta,
            Func<string, int, T> crear)
        {
            var totales = await _microservicios.ObtenerEstadisticaAsync(reporte);
            if (totales == null)
                return new ApiResponse<List<T>> { IsSuccess = false, Message = Mensajes.MESSAGE_SERVICIO_NO_DISPONIBLE };

            if (totales.Count == 0)
                return new ApiResponse<List<T>> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_EMPTY };

            var nombres = await obtenerNombres(totales.Select(t => t.Id));
            var lista = totales
                .Select(t => crear(nombres.GetValueOrDefault(t.Id) ?? $"{etiqueta} #{t.Id}", t.Total))
                .ToList();

            _logger.LogInformation("Reporte {Reporte} generado con {Cantidad} registros", reporte, lista.Count);
            return new ApiResponse<List<T>> { IsSuccess = true, Message = Mensajes.MESSAGE_QUERY, Data = lista };
        }

        private async Task<Dictionary<int, string>> NombresEmpleadosAsync(IEnumerable<int> ids)
        {
            var nombres = new Dictionary<int, string>();
            foreach (var id in ids.Distinct())
            {
                var nombre = await _microservicios.ObtenerNombreUsuarioAsync(id);
                if (!string.IsNullOrWhiteSpace(nombre))
                    nombres[id] = nombre;
            }
            return nombres;
        }

        public async Task<ApiResponse<string>> AnalisisIA(string? prompt)
        {
            var tipo = Reglas.DetectarTipoAnalisis(prompt);
            if (tipo == TipoAnalisis.Invalido)
                return ReglaIncumplida<string>(Mensajes.MESSAGE_IA_FAILLED, "AnalisisIA");

            var datos = await ObtenerDatosAnalisisAsync(tipo);
            if (datos == null)
                return new ApiResponse<string> { IsSuccess = false, Message = Mensajes.MESSAGE_SERVICIO_NO_DISPONIBLE };

            if (string.IsNullOrWhiteSpace(datos))
                return ReglaIncumplida<string>(Mensajes.MESSAGE_IA_SIN_DATOS, "AnalisisIA");

            var promptFinal = $"Analiza estos datos y responde en máximo 3 líneas: {datos} Pregunta: {prompt}";

            string respuesta;
            try
            {
                respuesta = await _ollama.GenerateAsync(promptFinal);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
            {
                _logger.LogError(ex, "Error al generar el análisis con IA (Ollama) de tipo {TipoAnalisis}", tipo);
                return new ApiResponse<string> { IsSuccess = false, Message = Mensajes.MESSAGE_IA_NO_DISPONIBLE };
            }

            _logger.LogInformation("Análisis con IA de tipo {TipoAnalisis} generado", tipo);
            return new ApiResponse<string> { IsSuccess = true, Message = Mensajes.MESSAGE_IA, Data = respuesta };
        }

        private async Task<string?> ObtenerDatosAnalisisAsync(TipoAnalisis tipo)
        {
            switch (tipo)
            {
                case TipoAnalisis.Producto:
                {
                    var productos = await _negocioRepository.ObtenerProductosActivosAsync();
                    var stocks = await _microservicios.ObtenerStocksAsync();
                    if (stocks == null) return null;

                    var stockPorProducto = stocks.ToDictionary(s => s.IdProducto, s => s.Stock);
                    return string.Join("\n", productos.Select(p => $"{p.Value}: {stockPorProducto.GetValueOrDefault(p.Key)} unidades en stock"));
                }

                case TipoAnalisis.Categoria:
                    return string.Join("\n", await _negocioRepository.ObtenerNombresCategoriasAsync());

                case TipoAnalisis.Cliente:
                    return await TextoReporteAsync(ObtenerTopClientes(), x => $"{x.Nombre_Completo}: {x.Compras_Totales} compras");

                case TipoAnalisis.Proveedor:
                    return await TextoReporteAsync(ObtenerTopProveedores(), x => $"{x.Nombre_Completo}: {x.Compras_Totales} compras");

                case TipoAnalisis.Comprado:
                    return await TextoTendenciaAsync("tendencia-comprados", "unidades compradas");

                case TipoAnalisis.Vendido:
                    return await TextoTendenciaAsync("tendencia-vendidos", "unidades vendidas");

                default:
                    return "";
            }
        }

        private static async Task<string?> TextoReporteAsync<T>(Task<ApiResponse<List<T>>> reporte, Func<T, string> formato)
        {
            var respuesta = await reporte;
            if (respuesta.Message == Mensajes.MESSAGE_SERVICIO_NO_DISPONIBLE) return null;
            return string.Join("\n", (respuesta.Data ?? []).Select(formato));
        }

        private async Task<string?> TextoTendenciaAsync(string reporte, string unidad)
        {
            var tendencia = await _microservicios.ObtenerTendenciaAsync(reporte);
            if (tendencia == null) return null;

            var nombres = await _negocioRepository.ObtenerNombresProductosAsync(tendencia.Select(t => t.IdProducto));
            return string.Join("\n", tendencia.Select(t =>
                $"{nombres.GetValueOrDefault(t.IdProducto) ?? $"Producto #{t.IdProducto}"} ({t.Fecha:dd/MM/yyyy}): {t.Cantidad} {unidad}"));
        }

        private ApiResponse<T> ReglaIncumplida<T>(string? mensaje, string operacion, object? id = null)
        {
            _logger.LogWarning("Regla de negocio incumplida en {Operacion} (Id: {Id}): {Mensaje}", operacion, id, mensaje);
            return new ApiResponse<T> { IsSuccess = false, Message = mensaje };
        }
    }
}
