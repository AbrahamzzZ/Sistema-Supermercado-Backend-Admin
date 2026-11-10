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
    public class ClienteService : IClienteService
    {
        private readonly ClienteRepository _clienteRepository;
        private readonly IValidator<Cliente> _validator;
        private readonly ILogger<ClienteService> _logger;

        public ClienteService(ClienteRepository clienteRepository, IValidator<Cliente> validator, ILogger<ClienteService> logger)
        {
            _logger = logger;
            _clienteRepository = clienteRepository;
            _validator = validator;
        }

        public async Task<ApiResponse<List<Cliente>>> ListarClientesAsync()
        {
            var listaClientes = await _clienteRepository.ListarClientesAsync();

            if (listaClientes == null || listaClientes.Count == 0)
                return new ApiResponse<List<Cliente>> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_EMPTY, Data = listaClientes };

            return new ApiResponse<List<Cliente>> { IsSuccess = true, Message = Mensajes.MESSAGE_QUERY, Data = listaClientes };
        }

        public async Task<ApiResponse<Paginacion<Cliente>>> ListarClientesPaginacionAsync(int pageNumber, int pageSize)
        {
            var pagedResult = await _clienteRepository.ListarClientesPaginacionAsync(pageNumber, pageSize);

            if (pagedResult.Items == null || pagedResult.Items.Count == 0)
            {
                return new ApiResponse<Paginacion<Cliente>> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_EMPTY, Data = pagedResult };
            }

            return new ApiResponse<Paginacion<Cliente>> { IsSuccess = true, Message = Mensajes.MESSAGE_QUERY, Data = pagedResult };
        }

        public async Task<ApiResponse<Cliente>> ObtenerClienteAsync(int idCliente)
        {
            var cliente = await _clienteRepository.ObtenerClienteAsync(idCliente);

            if (cliente == null)
            {
                return new ApiResponse<Cliente> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_NOT_FOUND };
            }

            return new ApiResponse<Cliente> { IsSuccess = true, Message = Mensajes.MESSAGE_QUERY, Data = cliente };
        }

        public async Task<ApiResponse<object>> RegistrarClienteAsync(Cliente cliente)
        {
            var validationResult = await _validator.ValidateAsync(cliente);

            if (!validationResult.IsValid)
                return ReglaIncumplida<object>(string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage)), "RegistrarClienteAsync", cliente?.Id_Cliente);

            var clientes = await _clienteRepository.ListarClientesAsync();
            if (clientes.Any(c => c.Codigo == cliente.Codigo))
                return ReglaIncumplida<object>("El código ya existe", "RegistrarClienteAsync", cliente?.Id_Cliente);

            if (clientes.Any(c => c.Cedula == cliente.Cedula))
                return ReglaIncumplida<object>("El cédula ya existe", "RegistrarClienteAsync", cliente?.Id_Cliente);

            if (clientes.Any(c => c.Telefono == cliente.Telefono))
                return ReglaIncumplida<object>("El télefono ya existe", "RegistrarClienteAsync", cliente?.Id_Cliente);

            var result = await _clienteRepository.RegistrarClienteAsync(cliente);
            if (result > 0)
                return new ApiResponse<object> { IsSuccess = true, Message = Mensajes.MESSAGE_REGISTER };

            return ReglaIncumplida<object>(Mensajes.MESSAGE_REGISTER_FAILLED, "RegistrarClienteAsync", cliente?.Id_Cliente);
        }


        public async Task<ApiResponse<object>> EditarClienteAsync(Cliente cliente)
        {
            if (cliente == null)
                return ReglaIncumplida<object>(Mensajes.MESSAGE_NULL, "EditarClienteAsync", cliente?.Id_Cliente);

            var validationResult = await _validator.ValidateAsync(cliente);
            if (!validationResult.IsValid)
                return ReglaIncumplida<object>(string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage)), "EditarClienteAsync", cliente?.Id_Cliente);

            var clienteExistente = await _clienteRepository.ObtenerClienteAsync(cliente.Id_Cliente);
            if (clienteExistente == null)
                return ReglaIncumplida<object>(Mensajes.MESSAGE_QUERY_NOT_FOUND, "EditarClienteAsync", cliente?.Id_Cliente);

            var clientes = await _clienteRepository.ListarClientesAsync();
            if (clientes.Any(c =>c.Cedula == cliente.Cedula && c.Id_Cliente != cliente.Id_Cliente))
            {
                return ReglaIncumplida<object>("El cédula ya existe.", "EditarClienteAsync", cliente?.Id_Cliente);
            }else if (clientes.Any(c => c.Telefono == cliente.Telefono && c.Id_Cliente != cliente.Id_Cliente))
            {
                return ReglaIncumplida<object>("El teléfono ya existe.", "EditarClienteAsync", cliente?.Id_Cliente);
            }

            var result = await _clienteRepository.EditarClienteAsync(cliente);
            if (result > 0)
                return new ApiResponse<object> { IsSuccess = true, Message = Mensajes.MESSAGE_UPDATE };

            return ReglaIncumplida<object>(Mensajes.MESSAGE_UPDATE_FAILLED, "EditarClienteAsync", cliente?.Id_Cliente);
        }

        public async Task<ApiResponse<int>> EliminarClienteAsync(int id)
        {
            try
            {
                var existe = await _clienteRepository.ObtenerClienteAsync(id);
                if (existe == null)
                {
                    return ReglaIncumplida<int>(Mensajes.MESSAGE_QUERY_NOT_FOUND, "EliminarClienteAsync", id);
                }

                var result = await _clienteRepository.EliminarClienteAsync(id);

                if (result > 0)
                    return new ApiResponse<int> { IsSuccess = true, Message = Mensajes.MESSAGE_DELETE };

                return ReglaIncumplida<int>(Mensajes.MESSAGE_DELETE_FAILLED, "EliminarClienteAsync", id);
            }
            catch (SqlException ex) when (ex.Number is 547 or 50547)
            {
                return ReglaIncumplida<int>("No se puede eliminar al cliente porque tiene ventas asociadas.", "EliminarClienteAsync", id);
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
