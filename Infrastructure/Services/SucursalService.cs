using Domain.Model;
using Infrastructure.Repository.InterfacesRepository;
using FluentValidation;
using Infrastructure.Repository;
using Infrastructure.Repository.InterfacesServices;
using Microsoft.Extensions.Logging;
using Utilities.Shared;
using Domain.Model.Dto;

namespace Infrastructure.Services
{
    public class SucursalService : ISucursalService
    {
        private readonly SucursalRepository _sucursalRepository;
        private readonly IValidator<SucursalUbicacion> _validator;
        private readonly ILogger<SucursalService> _logger;

        public SucursalService(SucursalRepository sucursalRepository, IValidator<SucursalUbicacion> validator, ILogger<SucursalService> logger)
        {
            _logger = logger;
            _sucursalRepository = sucursalRepository;
            _validator = validator;
        }

        public async Task<ApiResponse<List<SucursalUbicacion>>> ListarSucursalesAsync()
        {
            var listaSucursales = await _sucursalRepository.ListarSucursalesAsync();

            if (listaSucursales == null || listaSucursales.Count == 0)
                return new ApiResponse<List<SucursalUbicacion>> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_EMPTY, Data = listaSucursales };

            return new ApiResponse<List<SucursalUbicacion>> { IsSuccess = true, Message = Mensajes.MESSAGE_QUERY, Data = listaSucursales };
        }

        public async Task<ApiResponse<Paginacion<SucursalUbicacion>>> ListarSucursalesPaginacionAsync(int pageNumber, int pageSize)
        {
            var pagedResult = await _sucursalRepository.ListarSucursalesPaginacionAsync(pageNumber, pageSize);

            if (pagedResult.Items == null || pagedResult.Items.Count == 0)
            {
                return new ApiResponse<Paginacion<SucursalUbicacion>> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_EMPTY, Data = pagedResult };
            }

            return new ApiResponse<Paginacion<SucursalUbicacion>> { IsSuccess = true, Message = Mensajes.MESSAGE_QUERY, Data = pagedResult };
        }

        public async Task<ApiResponse<SucursalUbicacion>> ObtenerSucursalAsync(int idSucursal)
        {
            var sucursal = await _sucursalRepository.ObtenerSucursalAsync(idSucursal); 

            if (sucursal == null)
            {
                return new ApiResponse<SucursalUbicacion> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_NOT_FOUND };
            }

            return new ApiResponse<SucursalUbicacion> { IsSuccess = true, Message = Mensajes.MESSAGE_QUERY, Data = sucursal };
        }

        public async Task<ApiResponse<object>> RegistrarSucursalAsync(SucursalUbicacion sucursal)
        {
            var validationResult = await _validator.ValidateAsync(sucursal);

            if (!validationResult.IsValid)
                return ReglaIncumplida<object>(string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage)), "RegistrarSucursalAsync", sucursal?.Id_Sucursal);

            var sucursales = await _sucursalRepository.ListarSucursalesAsync();
            if (sucursales.Any(c => c.Codigo == sucursal.Codigo))
                return ReglaIncumplida<object>("El código ya existe", "RegistrarSucursalAsync", sucursal?.Id_Sucursal);

            if (sucursales.Any(c => c.Nombre_Sucursal?.ToLower() == sucursal.Nombre_Sucursal?.ToLower()))
                return ReglaIncumplida<object>("El nombre ya existe", "RegistrarSucursalAsync", sucursal?.Id_Sucursal);

            var result = await _sucursalRepository.RegistrarSucursalAsync(sucursal);
            if (result > 0)
                return new ApiResponse<object> { IsSuccess = true, Message = Mensajes.MESSAGE_REGISTER };

            return ReglaIncumplida<object>(Mensajes.MESSAGE_REGISTER_FAILLED, "RegistrarSucursalAsync", sucursal?.Id_Sucursal);
        }

        public async Task<ApiResponse<object>> EditarSucursalAsync(SucursalUbicacion sucursal)
        {
            if (sucursal == null)
                return ReglaIncumplida<object>(Mensajes.MESSAGE_NULL, "EditarSucursalAsync", sucursal?.Id_Sucursal);

            var validationResult = await _validator.ValidateAsync(sucursal);
            if (!validationResult.IsValid)
                return ReglaIncumplida<object>(string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage)), "EditarSucursalAsync", sucursal?.Id_Sucursal);

            var sucursalExistente = await _sucursalRepository.ObtenerSucursalAsync(sucursal.Id_Sucursal);
            if (sucursalExistente == null)
                return ReglaIncumplida<object>(Mensajes.MESSAGE_QUERY_NOT_FOUND, "EditarSucursalAsync", sucursal?.Id_Sucursal);

            var categorias = await _sucursalRepository.ListarSucursalesAsync();
            if (categorias.Any(c =>
                c.Nombre_Sucursal?.ToLower() == sucursal.Nombre_Sucursal?.ToLower()
                && c.Id_Sucursal != sucursal.Id_Sucursal))
            {
                return ReglaIncumplida<object>("El nombre ya existe.", "EditarSucursalAsync", sucursal?.Id_Sucursal);
            }

            var result = await _sucursalRepository.EditarSucursalAsync(sucursal);
            if (result > 0)
                return new ApiResponse<object> { IsSuccess = true, Message = Mensajes.MESSAGE_UPDATE };

            return ReglaIncumplida<object>(Mensajes.MESSAGE_UPDATE_FAILLED, "EditarSucursalAsync", sucursal?.Id_Sucursal);
        }

        public async Task<ApiResponse<int>> EliminarSucursalAsync(int id)
        {
            var existe = await _sucursalRepository.ObtenerSucursalAsync(id);
            if (existe == null)
            {
                return ReglaIncumplida<int>(Mensajes.MESSAGE_QUERY_NOT_FOUND, "EliminarSucursalAsync", id);
            }

            var result = await _sucursalRepository.EliminarSucursalAsync(id);

            if (result > 0)
                return new ApiResponse<int> { IsSuccess = true, Message = Mensajes.MESSAGE_DELETE };

            return ReglaIncumplida<int>(Mensajes.MESSAGE_DELETE_FAILLED, "EliminarSucursalAsync", id);
        }
        /// <summary>Regla de negocio incumplida: se registra como Warning (Information = éxito, Error = fallo técnico).</summary>
        private ApiResponse<T> ReglaIncumplida<T>(string? mensaje, string operacion, object? id = null)
        {
            _logger.LogWarning("Regla de negocio incumplida en {Operacion} (Id: {Id}): {Mensaje}", operacion, id, mensaje);
            return new ApiResponse<T> { IsSuccess = false, Message = mensaje };
        }

    }
}
