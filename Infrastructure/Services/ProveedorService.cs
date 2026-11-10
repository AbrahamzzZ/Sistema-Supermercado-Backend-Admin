using Domain.Model;
using Infrastructure.Repository.InterfacesRepository;
using FluentValidation;
using Infrastructure.Repository;
using Infrastructure.Repository.InterfacesServices;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Utilities.Shared;

namespace Infrastructure.Services
{
    public class ProveedorService : IProveedorService
    {
        private readonly ProveedorRepository _proveedorRepository;
        private readonly IValidator<Proveedor> _validator;
        private readonly ILogger<ProveedorService> _logger;

        public ProveedorService(ProveedorRepository proveedorRepository, IValidator<Proveedor> validator, ILogger<ProveedorService> logger)
        {
            _logger = logger;
            _proveedorRepository = proveedorRepository;
            _validator = validator;
        }

        public async Task<ApiResponse<List<Proveedor>>> ListarProveedoresAsync()
        {
            var listaProveedores = await _proveedorRepository.ListarProveedoresAsync();

            if (listaProveedores == null || listaProveedores.Count == 0)
                return new ApiResponse<List<Proveedor>> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_EMPTY, Data = listaProveedores };

            return new ApiResponse<List<Proveedor>> { IsSuccess = true, Message = Mensajes.MESSAGE_QUERY, Data = listaProveedores };
        }

        public async Task<ApiResponse<Paginacion<Proveedor>>> ListarProveedoresPaginacionAsync(int pageNumber, int pageSize)
        {
            var pagedResult = await _proveedorRepository.ListarProveedoresPaginacionAsync(pageNumber, pageSize);

            if (pagedResult.Items == null || pagedResult.Items.Count == 0)
            {
                return new ApiResponse<Paginacion<Proveedor>> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_EMPTY, Data = pagedResult };
            }

            return new ApiResponse<Paginacion<Proveedor>> { IsSuccess = true, Message = Mensajes.MESSAGE_QUERY, Data = pagedResult };
        }

        public async Task<ApiResponse<Proveedor>> ObtenerProveedorAsync(int idProveedor)
        {
            var proveedor = await _proveedorRepository.ObtenerProveedorAsync(idProveedor);

            if (proveedor == null)
            {
                return new ApiResponse<Proveedor> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_NOT_FOUND };
            }

            return new ApiResponse<Proveedor> { IsSuccess = true, Message = Mensajes.MESSAGE_QUERY, Data = proveedor };
        }

        public async Task<ApiResponse<object>> RegistrarProveedorAsync(Proveedor proveedor)
        {
            var validationResult = await _validator.ValidateAsync(proveedor);

            if (!validationResult.IsValid)
                return ReglaIncumplida<object>(string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage)), "RegistrarProveedorAsync", proveedor?.Id_Proveedor);

            var categorias = await _proveedorRepository.ListarProveedoresAsync();
            if (categorias.Any(c => c.Codigo == proveedor.Codigo))
                return ReglaIncumplida<object>("El código ya existe", "RegistrarProveedorAsync", proveedor?.Id_Proveedor);

            if (categorias.Any(c => c.Cedula == proveedor.Cedula))
                return ReglaIncumplida<object>("El cédula ya existe", "RegistrarProveedorAsync", proveedor?.Id_Proveedor);

            if (categorias.Any(c => c.Telefono == proveedor.Telefono))
                return ReglaIncumplida<object>("El télefono ya existe", "RegistrarProveedorAsync", proveedor?.Id_Proveedor);

            var result = await _proveedorRepository.RegistrarProveedorAsync(proveedor);
            if (result > 0)
                return new ApiResponse<object> { IsSuccess = true, Message = Mensajes.MESSAGE_REGISTER };

            return ReglaIncumplida<object>(Mensajes.MESSAGE_REGISTER_FAILLED, "RegistrarProveedorAsync", proveedor?.Id_Proveedor);

        }

        public async Task<ApiResponse<object>> EditarProveedorAsync(Proveedor proveedor)
        {
            if (proveedor == null)
                return ReglaIncumplida<object>(Mensajes.MESSAGE_NULL, "EditarProveedorAsync", proveedor?.Id_Proveedor);

            var validationResult = await _validator.ValidateAsync(proveedor);
            if (!validationResult.IsValid)
                return ReglaIncumplida<object>(string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage)), "EditarProveedorAsync", proveedor?.Id_Proveedor);

            var proveedorExistente = await _proveedorRepository.ObtenerProveedorAsync(proveedor.Id_Proveedor);
            if (proveedorExistente == null)
                return ReglaIncumplida<object>(Mensajes.MESSAGE_QUERY_NOT_FOUND, "EditarProveedorAsync", proveedor?.Id_Proveedor);

            var clientes = await _proveedorRepository.ListarProveedoresAsync();
            if (clientes.Any(c => c.Cedula == proveedor.Cedula && c.Id_Proveedor != proveedor.Id_Proveedor))
            {
                return ReglaIncumplida<object>("El cédula ya existe.", "EditarProveedorAsync", proveedor?.Id_Proveedor);
            }
            else if (clientes.Any(c => c.Telefono == proveedor.Telefono && c.Id_Proveedor != proveedor.Id_Proveedor))
            {
                return ReglaIncumplida<object>("El teléfono ya existe.", "EditarProveedorAsync", proveedor?.Id_Proveedor);
            }

            var result = await _proveedorRepository.EditarProveedorAsync(proveedor);
            if (result > 0)
                return new ApiResponse<object> { IsSuccess = true, Message = Mensajes.MESSAGE_UPDATE };

            return ReglaIncumplida<object>(Mensajes.MESSAGE_UPDATE_FAILLED, "EditarProveedorAsync", proveedor?.Id_Proveedor);
        }

        public async Task<ApiResponse<int>> EliminarProveedorAsync(int id)
        {
            try
            {
                var existe = await _proveedorRepository.ObtenerProveedorAsync(id);
                if (existe == null)
                {
                    return ReglaIncumplida<int>(Mensajes.MESSAGE_QUERY_NOT_FOUND, "EliminarProveedorAsync", id);
                }

                var result = await _proveedorRepository.EliminarProveedorAsync(id);

                if (result > 0)
                    return new ApiResponse<int> { IsSuccess = true, Message = Mensajes.MESSAGE_DELETE };

                return ReglaIncumplida<int>(Mensajes.MESSAGE_DELETE_FAILLED, "EliminarProveedorAsync", id);
            }
            catch (SqlException ex) when (ex.Number is 547 or 50547)
            {
                return ReglaIncumplida<int>("No se puede eliminar al proveedor porque tiene compras asociadas.", "EliminarProveedorAsync", id);
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
