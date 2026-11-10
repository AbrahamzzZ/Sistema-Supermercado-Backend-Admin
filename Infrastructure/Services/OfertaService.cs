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
    public class OfertaService : IOfertaService
    {
        private readonly OfertaRepository _ofertaRepository;
        private readonly IValidator<Oferta> _validator;
        private readonly ILogger<OfertaService> _logger;

        public OfertaService(OfertaRepository ofertaRepository, IValidator<Oferta> validator, ILogger<OfertaService> logger)
        {
            _logger = logger;
            _ofertaRepository = ofertaRepository;
            _validator = validator;
        }

        public async Task<ApiResponse<List<OfertaProducto>>> ListarOfertasAsync()
        {
            var listaOfertas = await _ofertaRepository.ListarOfertasAsync();

            if (listaOfertas == null || listaOfertas.Count == 0)
                return new ApiResponse<List<OfertaProducto>> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_EMPTY, Data = listaOfertas };

            return new ApiResponse<List<OfertaProducto>> { IsSuccess = true, Message = Mensajes.MESSAGE_QUERY, Data = listaOfertas };
        }

        public async Task<ApiResponse<Paginacion<OfertaProducto>>> ListarOfertasPaginacionAsync(int pageNumber, int pageSize)
        {
            var pagedResult = await _ofertaRepository.ListarOfertasPaginacionAsync(pageNumber, pageSize);

            if (pagedResult.Items == null || pagedResult.Items.Count == 0)
            {
                return new ApiResponse<Paginacion<OfertaProducto>> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_EMPTY, Data = pagedResult };
            }

            return new ApiResponse<Paginacion<OfertaProducto>> { IsSuccess = true, Message = Mensajes.MESSAGE_QUERY, Data = pagedResult };
        }

        public async Task<ApiResponse<Oferta>> ObtenerOfertaAsync(int idOferta)
        {
            var oferta = await _ofertaRepository.ObtenerOfertaAsync(idOferta);

            if (oferta == null)
            {
                return new ApiResponse<Oferta> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_NOT_FOUND };
            }

            return new ApiResponse<Oferta> { IsSuccess = true, Message = Mensajes.MESSAGE_QUERY, Data = oferta };
        }

        public async Task<ApiResponse<object>> RegistrarOfertaAsync(Oferta oferta)
        {
            var validationResult = await _validator.ValidateAsync(oferta);

            if (!validationResult.IsValid)
                return ReglaIncumplida<object>(string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage)), "RegistrarOfertaAsync", oferta?.Id_Oferta);

            var ofertas = await _ofertaRepository.ListarOfertasAsync();
            if (ofertas.Any(c => c.Codigo == oferta.Codigo))
                return ReglaIncumplida<object>("El código ya existe", "RegistrarOfertaAsync", oferta?.Id_Oferta);

            if (ofertas.Any(c => c.Nombre_Oferta?.ToLower() == oferta.Nombre_Oferta?.ToLower()))
                return ReglaIncumplida<object>("El nombre ya existe", "RegistrarOfertaAsync", oferta?.Id_Oferta);

            var result = await _ofertaRepository.RegistrarOfertaAsync(oferta);
            if (result > 0)
                return new ApiResponse<object> { IsSuccess = true, Message = Mensajes.MESSAGE_REGISTER };

            return ReglaIncumplida<object>(Mensajes.MESSAGE_REGISTER_FAILLED, "RegistrarOfertaAsync", oferta?.Id_Oferta);
        }

        public async Task<ApiResponse<object>> EditarOfertaAsync(Oferta oferta)
        {
            if (oferta == null)
                return ReglaIncumplida<object>(Mensajes.MESSAGE_NULL, "EditarOfertaAsync", oferta?.Id_Oferta);

            var validationResult = await _validator.ValidateAsync(oferta);
            if (!validationResult.IsValid)
                return ReglaIncumplida<object>(string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage)), "EditarOfertaAsync", oferta?.Id_Oferta);

            var ofertas = await _ofertaRepository.ListarOfertasAsync();
            if (ofertas.Any(c => c.Nombre_Oferta == oferta.Nombre_Oferta && c.Id_Oferta != oferta.Id_Oferta))
            {
                return ReglaIncumplida<object>("El nombre ya existe.", "EditarOfertaAsync", oferta?.Id_Oferta);
            }

            var result = await _ofertaRepository.EditarOfertaAsync(oferta);
            if (result > 0)
                return new ApiResponse<object> { IsSuccess = true, Message = Mensajes.MESSAGE_UPDATE };

            return ReglaIncumplida<object>(Mensajes.MESSAGE_UPDATE_FAILLED, "EditarOfertaAsync", oferta?.Id_Oferta);
        }

        public async Task<ApiResponse<int>> EliminarOfertaAsync(int id)
        {
            try
            {
                var existe = await _ofertaRepository.ObtenerOfertaAsync(id);
                if (existe == null)
                {
                    return ReglaIncumplida<int>(Mensajes.MESSAGE_QUERY_NOT_FOUND, "EliminarOfertaAsync", id);
                }

                var result = await _ofertaRepository.EliminarOfertaAsync(id);

                if (result > 0)
                    return new ApiResponse<int> { IsSuccess = true, Message = Mensajes.MESSAGE_DELETE };

                return ReglaIncumplida<int>(Mensajes.MESSAGE_DELETE_FAILLED, "EliminarOfertaAsync", id);
            }
            catch (SqlException ex) when (ex.Number is 547 or 50547)
            {
                return ReglaIncumplida<int>("No se puede eliminar la oferta porque tiene ventas asociadas.", "EliminarOfertaAsync", id);
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
