using Domain.Model.Dto.Negocio;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using Utilities.Shared;

namespace Infrastructure.Services
{
    /// <summary>
    /// Consultas a otros microservicios a través del Gateway, para los reportes del negocio:
    /// </summary>
    public class MicroserviciosApiClient
    {
        public const string NombreHttpClient = "Gateway";

        private readonly HttpClient _client;
        private readonly ILogger<MicroserviciosApiClient> _logger;

        public MicroserviciosApiClient(IHttpClientFactory factory, ILogger<MicroserviciosApiClient> logger)
        {
            _client = factory.CreateClient(NombreHttpClient);
            _logger = logger;
        }

        /// <summary>Totales por ID desde Order: /order/Estadistica/{reporte}</summary>
        public Task<List<EstadisticaTotal>?> ObtenerEstadisticaAsync(string reporte)
            => GetAsync<List<EstadisticaTotal>>($"/order/Estadistica/{reporte}");

        /// <summary>Unidades por producto y día desde Order: /order/Estadistica/tendencia-{vendidos|comprados}</summary>
        public Task<List<EstadisticaTendencia>?> ObtenerTendenciaAsync(string reporte)
            => GetAsync<List<EstadisticaTendencia>>($"/order/Estadistica/{reporte}");

        /// <summary>Stock de todos los productos desde Inventory</summary>
        public Task<List<StockProducto>?> ObtenerStocksAsync()
            => GetAsync<List<StockProducto>>("/inventory/ProductoStock");

        /// <summary>Nombre de un usuario (empleado) desde Auth; null si no existe o falla</summary>
        public async Task<string?> ObtenerNombreUsuarioAsync(int idUsuario)
            => (await GetAsync<UsuarioNombre>($"/auth/Usuario/{idUsuario}", registrarNoEncontrado: false))?.Nombre_Completo;

        private async Task<T?> GetAsync<T>(string url, bool registrarNoEncontrado = true) where T : class
        {
            try
            {
                var response = await _client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    if (registrarNoEncontrado || (int)response.StatusCode >= 500)
                        _logger.LogError("La consulta a {Url} respondió {StatusCode}", url, (int)response.StatusCode);
                    return null;
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
                return apiResponse?.Data;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
            {
                _logger.LogError(ex, "No se pudo consultar {Url}", url);
                return null;
            }
        }
    }
}
