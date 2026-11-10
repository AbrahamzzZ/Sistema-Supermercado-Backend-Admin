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
        private readonly ProductoRepository _productoRepository;
        private readonly IValidator<Producto> _validator;
        private readonly ILogger<ProductoService> _logger;

        public ProductoService(ProductoRepository productoRepository, IValidator<Producto> validator, ILogger<ProductoService> logger)
        {
            _logger = logger;
            _productoRepository = productoRepository;
            _validator = validator;
        }

        public async Task<ApiResponse<List<ProductoCategoria>>> ListarProductosAsync()
        {
            var listapProductos = await _productoRepository.ListarProductosAsync();

            if (listapProductos == null || listapProductos.Count == 0)
                return new ApiResponse<List<ProductoCategoria>> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_EMPTY, Data = listapProductos };

            return new ApiResponse<List<ProductoCategoria>> { IsSuccess = true, Message = Mensajes.MESSAGE_QUERY, Data = listapProductos };
        }

        public async Task<ApiResponse<Paginacion<ProductoCategoria>>> ListarProductosPaginacionAsync(int pageNumber, int pageSize)
        {
            var pagedResult = await _productoRepository.ListarProductosPaginacionAsync(pageNumber, pageSize);

            if (pagedResult.Items == null || pagedResult.Items.Count == 0)
            {
                return new ApiResponse<Paginacion<ProductoCategoria>> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_EMPTY, Data = pagedResult };
            }

            return new ApiResponse<Paginacion<ProductoCategoria>> { IsSuccess = true, Message = Mensajes.MESSAGE_QUERY, Data = pagedResult };
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
