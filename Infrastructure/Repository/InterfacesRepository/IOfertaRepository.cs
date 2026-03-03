using Domain.Model;
using Domain.Model.Dto;
using Utilities.Shared;

namespace Infrastructure.Repository.InterfacesRepository
{
    public interface IOfertaRepository
    {
        Task<List<OfertaProducto>> ListarOfertasAsync();
        Task<Paginacion<OfertaProducto>> ListarOfertasPaginacionAsync(int pageNumber, int pageSize);
        Task<Oferta?> ObtenerOfertaAsync(int idOferta);
        Task<int> RegistrarOfertaAsync(Oferta oferta);
        Task<int> EditarOfertaAsync(Oferta oferta);
        Task<int> EliminarOfertaAsync(int idOferta);
    }
}
