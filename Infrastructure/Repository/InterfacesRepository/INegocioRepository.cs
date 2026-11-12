using Domain.Model;

namespace Infrastructure.Repository.InterfacesRepository
{
    public interface INegocioRepository
    {
        Task<Negocio?> ObtenerNegocioAsync(int idNegocio);
        Task<int> EditarNegocioAsync(Negocio negocio);
        Task<Dictionary<int, string>> ObtenerNombresProductosAsync(IEnumerable<int> ids);
        Task<Dictionary<int, string>> ObtenerNombresClientesAsync(IEnumerable<int> ids);
        Task<Dictionary<int, string>> ObtenerNombresProveedoresAsync(IEnumerable<int> ids);
        Task<Dictionary<int, string>> ObtenerNombresTransportistasAsync(IEnumerable<int> ids);
        Task<Dictionary<int, string>> ObtenerProductosActivosAsync();
        Task<List<string>> ObtenerNombresCategoriasAsync();
    }
}
