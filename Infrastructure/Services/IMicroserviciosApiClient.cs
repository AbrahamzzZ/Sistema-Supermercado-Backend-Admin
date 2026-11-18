using Domain.Model.Dto.Negocio;

namespace Infrastructure.Services
{
    /// <summary>Consultas a Order, Inventory y Auth (a través del Gateway) para los reportes y el análisis con IA.</summary>
    public interface IMicroserviciosApiClient
    {
        Task<List<EstadisticaTotal>?> ObtenerEstadisticaAsync(string reporte);
        Task<List<EstadisticaTendencia>?> ObtenerTendenciaAsync(string reporte);
        Task<List<StockProducto>?> ObtenerStocksAsync();
        Task<string?> ObtenerNombreUsuarioAsync(int idUsuario);
    }
}
