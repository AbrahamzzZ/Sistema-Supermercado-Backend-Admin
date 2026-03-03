using Domain.Model;
using Utilities.Shared;

namespace Infrastructure.Repository.InterfacesRepository
{
    public interface ICategoriaRepository
    {
        Task<List<Categoria>> ListarCategoriasAsync();
        Task<Paginacion<Categoria>> ListarCategoriasPaginacionAsync(int pageNumber, int pageSize);
        Task<Categoria?> ObtenerCategoriaAsync(int idCategoria);
        Task<int> RegistrarCategoriaAsync(Categoria categoria);
        Task<int> EditarCategoriaAsync(Categoria categoria);
        Task<int> EliminarCategoriaAsync(int id);
    }
}
