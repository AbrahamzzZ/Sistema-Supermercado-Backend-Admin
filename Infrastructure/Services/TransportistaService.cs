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
    public class TransportistaService : ITransportistaService
    {
        private readonly TransportistaRepository _transportistaRepository;
        private readonly IValidator<Transportista> _validator;
        private readonly ILogger<TransportistaService> _logger;

        public TransportistaService(TransportistaRepository transportistaRepository, IValidator<Transportista> validator, ILogger<TransportistaService> logger)
        {
            _logger = logger;
            _transportistaRepository = transportistaRepository;
            _validator = validator;
        }

        public async Task<ApiResponse<List<Transportista>>> ListarTransportistasAsync()
        {
            var listaTransportistas = await _transportistaRepository.ListarTransportistasAsync();

            if (listaTransportistas == null || listaTransportistas.Count == 0)
                return new ApiResponse<List<Transportista>> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_EMPTY, Data = listaTransportistas };

            return new ApiResponse<List<Transportista>> { IsSuccess = true, Message = Mensajes.MESSAGE_QUERY, Data = listaTransportistas };
        }

        public async Task<ApiResponse<Paginacion<Transportista>>> ListarTransportistasPaginacionAsync(int pageNumber, int pageSize)
        {
            var pagedResult = await _transportistaRepository.ListarTransportistasPaginacionAsync(pageNumber, pageSize);

            if (pagedResult.Items == null || !pagedResult.Items.Any())
            {
                return new ApiResponse<Paginacion<Transportista>> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_EMPTY, Data = pagedResult };
            }

            return new ApiResponse<Paginacion<Transportista>> { IsSuccess = true, Message = Mensajes.MESSAGE_QUERY, Data = pagedResult };
        }

        public async Task<ApiResponse<Transportista>> ObtenerTransportistaAsync(int idTranportista)
        {
            var transportista = await _transportistaRepository.ObtenerTransportistaAsync(idTranportista);

            if (transportista == null)
            {
                return new ApiResponse<Transportista> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_NOT_FOUND };
            }

            return new ApiResponse<Transportista> { IsSuccess = true, Message = Mensajes.MESSAGE_QUERY, Data = transportista };
        }

        public async Task<ApiResponse<object>> RegistrarTransportistaAsync(Transportista transportista)
        {
            var validationResult = await _validator.ValidateAsync(transportista);

            if (!validationResult.IsValid)
                return ReglaIncumplida<object>(string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage)), "RegistrarTransportistaAsync", transportista?.Id_Transportista);

            var transportistas = await _transportistaRepository.ListarTransportistasAsync();
            if (transportistas.Any(c => c.Codigo == transportista.Codigo))
                return ReglaIncumplida<object>("El código ya existe", "RegistrarTransportistaAsync", transportista?.Id_Transportista);

            if (transportistas.Any(c => c.Cedula == transportista.Cedula))
                return ReglaIncumplida<object>("El cédula ya existe", "RegistrarTransportistaAsync", transportista?.Id_Transportista);

            if (transportistas.Any(c => c.Telefono == transportista.Telefono))
                return ReglaIncumplida<object>("El télefono ya existe", "RegistrarTransportistaAsync", transportista?.Id_Transportista);

            var result = await _transportistaRepository.RegistrarTransportistaAsync(transportista);
            if (result > 0)
                return new ApiResponse<object> { IsSuccess = true, Message = Mensajes.MESSAGE_REGISTER };

            return ReglaIncumplida<object>(Mensajes.MESSAGE_REGISTER_FAILLED, "RegistrarTransportistaAsync", transportista?.Id_Transportista);
        }

        public async Task<ApiResponse<object>> EditarTransportistaAsync(Transportista transportista)
        {
            if (transportista == null)
                return ReglaIncumplida<object>(Mensajes.MESSAGE_NULL, "EditarTransportistaAsync", transportista?.Id_Transportista);

            var validationResult = await _validator.ValidateAsync(transportista);
            if (!validationResult.IsValid)
                return ReglaIncumplida<object>(string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage)), "EditarTransportistaAsync", transportista?.Id_Transportista);

            var transportistaExistente = await _transportistaRepository.ObtenerTransportistaAsync(transportista.Id_Transportista);
            if (transportistaExistente == null)
                return ReglaIncumplida<object>(Mensajes.MESSAGE_QUERY_NOT_FOUND, "EditarTransportistaAsync", transportista?.Id_Transportista);

            var transportistas = await _transportistaRepository.ListarTransportistasAsync();
            if (transportistas.Any(c => c.Cedula == transportista.Cedula && c.Id_Transportista != transportista.Id_Transportista))
            {
                return ReglaIncumplida<object>("El cédula ya existe.", "EditarTransportistaAsync", transportista?.Id_Transportista);
            }
            else if (transportistas.Any(c => c.Telefono == transportista.Telefono && c.Id_Transportista != transportista.Id_Transportista))
            {
                return ReglaIncumplida<object>("El teléfono ya existe.", "EditarTransportistaAsync", transportista?.Id_Transportista);
            }

            var result = await _transportistaRepository.EditarTransportistaAsync(transportista);
            if (result > 0)
                return new ApiResponse<object> { IsSuccess = true, Message = Mensajes.MESSAGE_UPDATE };

            return ReglaIncumplida<object>(Mensajes.MESSAGE_UPDATE_FAILLED, "EditarTransportistaAsync", transportista?.Id_Transportista);
        }

        public async Task<ApiResponse<int>> EliminarTransportistaAsync(int id)
        {
            try
            {
                var existe = await _transportistaRepository.ObtenerTransportistaAsync(id);
                if (existe == null)
                {
                    return ReglaIncumplida<int>(Mensajes.MESSAGE_QUERY_NOT_FOUND, "EliminarTransportistaAsync", id);
                }

                var result = await _transportistaRepository.EliminarTransportistaAsync(id);

                if (result > 0)
                    return new ApiResponse<int> { IsSuccess = true, Message = Mensajes.MESSAGE_DELETE };

                return ReglaIncumplida<int>(Mensajes.MESSAGE_DELETE_FAILLED, "EliminarTransportistaAsync", id);
            }
            catch (SqlException ex) when (ex.Number is 547 or 50547)
            {
                return ReglaIncumplida<int>("No se puede eliminar al transportista porque tiene compras asociadas.", "EliminarTransportistaAsync", id);
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
