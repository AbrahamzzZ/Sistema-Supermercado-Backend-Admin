using Domain.Model;
using Utilities.Shared;

namespace Infrastructure.Repository.InterfacesRepository
{
    public interface ITransportistaRepository
    {
        Task<List<Transportista>> ListarTransportistasAsync();
        Task<Paginacion<Transportista>> ListarTransportistasPaginacionAsync(int pageNumber, int pageSize);
        Task<Transportista?> ObtenerTransportistaAsync(int idTranportista);
        Task<int> RegistrarTransportistaAsync(Transportista transportista);
        Task<int> EditarTransportistaAsync(Transportista transportista);
        Task<int> EliminarTransportistaAsync(int idTransportista);
    }
}
