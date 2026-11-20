using Domain.Model;
using Domain.Model.Dto;
using Infrastructure.Repository.InterfacesRepository;
using FluentValidation;
using Infrastructure.Repository;
using Infrastructure.Repository.InterfacesServices;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Utilities.Shared;

namespace Infrastructure.Services
{
    public class ProductoService : IProductoService
    {
        private readonly IProductoRepository _productoRepository;
        private readonly IMicroserviciosApiClient _microservicios;
        private readonly IValidator<Producto> _validator;
        private readonly ILogger<ProductoService> _logger;

        public ProductoService(
            IProductoRepository productoRepository,
            IMicroserviciosApiClient microservicios,
            IValidator<Producto> validator,
            ILogger<ProductoService> logger)
        {
            _logger = logger;
            _productoRepository = productoRepository;
            _microservicios = microservicios;
            _validator = validator;
        }

        public async Task<ApiResponse<List<ProductoCategoria>>> ListarProductosAsync()
        {
            var listaProductos = await _productoRepository.ListarProductosAsync();

            if (listaProductos == null || listaProductos.Count == 0)
                return new ApiResponse<List<ProductoCategoria>> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_EMPTY, Data = listaProductos };

            if (!await AgregarDatosInventarioAsync(listaProductos))
                return new ApiResponse<List<ProductoCategoria>> { IsSuccess = false, Message = Mensajes.MESSAGE_SERVICIO_NO_DISPONIBLE };

            return new ApiResponse<List<ProductoCategoria>> { IsSuccess = true, Message = Mensajes.MESSAGE_QUERY, Data = listaProductos };
        }

        public async Task<ApiResponse<Paginacion<ProductoCategoria>>> ListarProductosPaginacionAsync(int pageNumber, int pageSize)
        {
            var pagedResult = await _productoRepository.ListarProductosPaginacionAsync(pageNumber, pageSize);

            if (pagedResult.Items == null || pagedResult.Items.Count == 0)
            {
                return new ApiResponse<Paginacion<ProductoCategoria>> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_EMPTY, Data = pagedResult };
            }

            if (!await AgregarDatosInventarioAsync(pagedResult.Items))
                return new ApiResponse<Paginacion<ProductoCategoria>> { IsSuccess = false, Message = Mensajes.MESSAGE_SERVICIO_NO_DISPONIBLE };

            return new ApiResponse<Paginacion<ProductoCategoria>> { IsSuccess = true, Message = Mensajes.MESSAGE_QUERY, Data = pagedResult };
        }

        private async Task<bool> AgregarDatosInventarioAsync(IEnumerable<ProductoCategoria> productos)
        {
            var inventario = await _microservicios.ObtenerStocksAsync();
            if (inventario == null)
                return false;

            var inventarioPorProducto = inventario.ToDictionary(item => item.IdProducto);
            foreach (var producto in productos)
            {
                if (!inventarioPorProducto.TryGetValue(producto.Id_Producto, out var datos))
                {
                    producto.Stock = 0;
                    continue;
                }

                producto.Stock = datos.Stock;
                producto.PrecioCompra = datos.PrecioCompra;
                producto.PrecioVenta = datos.PrecioVenta;
            }

            return true;
        }

        public async Task<ApiResponse<Producto>> ObtenerProductoAsync(int idProducto)
        {
            var producto = await _productoRepository.ObtenerProductoAsync(idProducto);

            if (producto == null)
            {
                return new ApiResponse<Producto> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_NOT_FOUND };
            }

            return new ApiResponse<Producto> { IsSuccess = true, Message = Mensajes.MESSAGE_QUERY, Data = producto };
        }

        public async Task<ApiResponse<object>> RegistrarProductoAsync(Producto producto)
        {
            var validationResult = await _validator.ValidateAsync(producto);

            if (!validationResult.IsValid)
                return ReglaIncumplida<object>(string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage)), "RegistrarProductoAsync", producto?.Id_Producto);

            var productos = await _productoRepository.ListarProductosAsync();
            if (productos.Any(c => c.Codigo == producto.Codigo))
                return ReglaIncumplida<object>("El código ya existe", "RegistrarProductoAsync", producto?.Id_Producto);

            if (productos.Any(c => c.Nombre_Producto?.ToLower() == producto.Nombre_Producto?.ToLower()))
                return ReglaIncumplida<object>("El nombre ya existe", "RegistrarProductoAsync", producto?.Id_Producto);

            var result = await _productoRepository.RegistrarProductoAsync(producto);
            if (result > 0)
                return new ApiResponse<object> { IsSuccess = true, Message = Mensajes.MESSAGE_REGISTER };

            return ReglaIncumplida<object>(Mensajes.MESSAGE_REGISTER_FAILLED, "RegistrarProductoAsync", producto?.Id_Producto);
        }

        public async Task<ApiResponse<object>> EditarProductoAsync(Producto producto)
        {
            if (producto == null)
                return ReglaIncumplida<object>(Mensajes.MESSAGE_NULL, "EditarProductoAsync", producto?.Id_Producto);

            var validationResult = await _validator.ValidateAsync(producto);
            if (!validationResult.IsValid)
                return ReglaIncumplida<object>(string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage)), "EditarProductoAsync", producto?.Id_Producto);

            var productoExistente = await _productoRepository.ObtenerProductoAsync(producto.Id_Producto);
            if (productoExistente == null)
                return ReglaIncumplida<object>(Mensajes.MESSAGE_QUERY_NOT_FOUND, "EditarProductoAsync", producto?.Id_Producto);

            var productos = await _productoRepository.ListarProductosAsync();
            if (productos.Any(c => c.Nombre_Producto?.ToLower() == producto.Nombre_Producto?.ToLower() && c.Id_Producto != producto.Id_Producto))
            {
                return ReglaIncumplida<object>("El nombre ya existe.", "EditarProductoAsync", producto?.Id_Producto);
            }

            var result = await _productoRepository.EditarProductoAsync(producto);
            if (result > 0)
                return new ApiResponse<object> { IsSuccess = true, Message = Mensajes.MESSAGE_UPDATE };

            return ReglaIncumplida<object>(Mensajes.MESSAGE_UPDATE_FAILLED, "EditarProductoAsync", producto?.Id_Producto);
        }

        public async Task<ApiResponse<int>> EliminarProductoAsync(int id)
        {
            try
            {
                var existe = await _productoRepository.ObtenerProductoAsync(id);
                if (existe == null)
                {
                    return ReglaIncumplida<int>(Mensajes.MESSAGE_QUERY_NOT_FOUND, "EliminarProductoAsync", id);
                }

                var result = await _productoRepository.EliminarProductoAsync(id);

                if (result > 0)
                    return new ApiResponse<int> { IsSuccess = true, Message = Mensajes.MESSAGE_DELETE };

                return ReglaIncumplida<int>(Mensajes.MESSAGE_DELETE_FAILLED, "EliminarProductoAsync", id);

            }
            catch (SqlException ex) when (ex.Number is 547 or 50547)
            {
                return ReglaIncumplida<int>("No se puede eliminar el producto porque tiene ofertas activas asociadas.", "EliminarProductoAsync", id);
            }
        }
        /// <summary>Regla de negocio incumplida: se registra como Warning (Information = éxito, Error = fallo técnico).</summary>
        private ApiResponse<T> ReglaIncumplida<T>(string? mensaje, string operacion, object? id = null)
        {
            _logger.LogWarning("Regla de negocio incumplida en {Operacion} (Id: {Id}): {Mensaje}", operacion, id, mensaje);
            return new ApiResponse<T> { IsSuccess = false, Message = mensaje };
        }

    }
}
