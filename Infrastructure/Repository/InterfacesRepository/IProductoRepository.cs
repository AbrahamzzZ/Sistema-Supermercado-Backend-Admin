using Domain.Model;
using Domain.Model.Dto;
using Utilities.Shared;

namespace Infrastructure.Repository.InterfacesRepository
{
    public interface IProductoRepository
    {
        Task<List<ProductoCategoria>> ListarProductosAsync();
        Task<Paginacion<ProductoCategoria>> ListarProductosPaginacionAsync(int pageNumber, int pageSize);
        Task<Producto?> ObtenerProductoAsync(int idProducto);
        Task<int> RegistrarProductoAsync(Producto producto);
        Task<int> EditarProductoAsync(Producto producto);
        Task<int> EliminarProductoAsync(int idProducto);
    }
}
