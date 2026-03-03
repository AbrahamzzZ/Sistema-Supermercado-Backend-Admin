using Domain.Model;
using Utilities.Shared;

namespace Infrastructure.Repository.InterfacesServices
{
    public interface ICategoriaService
    {
        Task<ApiResponse<List<Categoria>>> ListarCategoriasAsync();
        Task<ApiResponse<Paginacion<Categoria>>> ListarCategoriasPaginacionAsync(int pageNumber, int pageSize);
        Task<ApiResponse<Categoria>> ObtenerCategoriaAsync(int idCategoria);
        Task<ApiResponse<object>> RegistrarCategoriaAsync(Categoria categoria);
        Task<ApiResponse<object>> EditarCategoriaAsync(Categoria categoria);
        Task<ApiResponse<int>> EliminarCategoriaAsync(int id);
    }
}
