using Domain.Model;
using Domain.Model.Dto;
using Utilities.Shared;

namespace Infrastructure.Repository.InterfacesServices
{
    public interface IOfertaService
    {
        Task<ApiResponse<List<OfertaProducto>>> ListarOfertasAsync();
        Task<ApiResponse<Paginacion<OfertaProducto>>> ListarOfertasPaginacionAsync(int pageNumber, int pageSize);
        Task<ApiResponse<Oferta>> ObtenerOfertaAsync(int idOferta);
        Task<ApiResponse<object>> RegistrarOfertaAsync(Oferta oferta);
        Task<ApiResponse<object>> EditarOfertaAsync(Oferta oferta);
        Task<ApiResponse<int>> EliminarOfertaAsync(int idOferta);
    }
}
