using Domain.Model;
using Domain.Model.Dto;
using Utilities.Shared;

namespace Infrastructure.Repository.InterfacesServices
{
    public interface ISucursalService
    {
        Task<ApiResponse<List<SucursalUbicacion>>> ListarSucursalesAsync();
        Task<ApiResponse<Paginacion<SucursalUbicacion>>> ListarSucursalesPaginacionAsync(int pageNumber, int pageSize);
        Task<ApiResponse<SucursalUbicacion>> ObtenerSucursalAsync(int idSucursal);
        Task<ApiResponse<object>> RegistrarSucursalAsync(SucursalUbicacion sucursal);
        Task<ApiResponse<object>> EditarSucursalAsync(SucursalUbicacion sucursal);
        Task<ApiResponse<int>> EliminarSucursalAsync(int idSucursal);
    }
}
