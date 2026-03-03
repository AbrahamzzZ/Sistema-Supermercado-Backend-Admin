using Domain.Model;
using Domain.Model.Dto;
using Utilities.Shared;

namespace Infrastructure.Repository.InterfacesRepository
{
    public interface ISucursalRepository
    {
        Task<List<SucursalUbicacion>> ListarSucursalesAsync();
        Task<Paginacion<SucursalUbicacion>> ListarSucursalesPaginacionAsync(int pageNumber, int pageSize);
        Task<SucursalUbicacion?> ObtenerSucursalAsync(int idSucursal);
        Task<int> RegistrarSucursalAsync(SucursalUbicacion sucursal);
        Task<int> EditarSucursalAsync(SucursalUbicacion sucursal);
        Task<int> EliminarSucursalAsync(int idSucursal);
    }
}
