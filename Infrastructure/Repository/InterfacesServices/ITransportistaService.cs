using Domain.Model;
using Utilities.Shared;

namespace Infrastructure.Repository.InterfacesServices
{
    public interface ITransportistaService
    {
        Task<ApiResponse<List<Transportista>>> ListarTransportistasAsync();
        Task<ApiResponse<Paginacion<Transportista>>> ListarTransportistasPaginacionAsync(int pageNumber, int pageSize);
        Task<ApiResponse<Transportista>> ObtenerTransportistaAsync(int idTranportista);
        Task<ApiResponse<object>> RegistrarTransportistaAsync(Transportista transportista);
        Task<ApiResponse<object>> EditarTransportistaAsync(Transportista transportista);
        Task<ApiResponse<int>> EliminarTransportistaAsync(int idTransportista);
    }
}
