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
    public class CategoriaService : ICategoriaService
    {
        private readonly CategoriaRepository _categoriaRepository;
        private readonly IValidator<Categoria> _validator;
        private readonly ILogger<CategoriaService> _logger;

        public CategoriaService(CategoriaRepository categoriaRepository, IValidator<Categoria> validator, ILogger<CategoriaService> logger)
        {
            _logger = logger;
            _categoriaRepository = categoriaRepository;
            _validator = validator;
        }

        public async Task<ApiResponse<List<Categoria>>> ListarCategoriasAsync()
        {
            var listaCategorias = await _categoriaRepository.ListarCategoriasAsync();

            if (listaCategorias == null || listaCategorias.Count == 0)
                return new ApiResponse<List<Categoria>> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_EMPTY, Data = listaCategorias };

            return new ApiResponse<List<Categoria>> { IsSuccess = true, Message = Mensajes.MESSAGE_QUERY, Data = listaCategorias };
        }

        public async Task<ApiResponse<Paginacion<Categoria>>> ListarCategoriasPaginacionAsync(int pageNumber, int pageSize)
        {
            var pagedResult = await _categoriaRepository.ListarCategoriasPaginacionAsync(pageNumber, pageSize);

            if (pagedResult.Items == null || pagedResult.Items.Count == 0)
            {
                return new ApiResponse<Paginacion<Categoria>> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_EMPTY, Data = pagedResult };
            }

            return new ApiResponse<Paginacion<Categoria>> { IsSuccess = true, Message = Mensajes.MESSAGE_QUERY, Data = pagedResult };
        }

        public async Task<ApiResponse<Categoria>> ObtenerCategoriaAsync(int idCategoria)
        {
            var categoria = await _categoriaRepository.ObtenerCategoriaAsync(idCategoria);

            if (categoria == null)
            {
                return new ApiResponse<Categoria> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_NOT_FOUND };
            }

            return new ApiResponse<Categoria> { IsSuccess = true, Message = Mensajes.MESSAGE_QUERY, Data = categoria };
        }

        public async Task<ApiResponse<object>> RegistrarCategoriaAsync(Categoria categoria)
        {
            var validationResult = await _validator.ValidateAsync(categoria);

            if (!validationResult.IsValid)
                return ReglaIncumplida<object>(string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage)), "RegistrarCategoriaAsync", categoria?.Id_Categoria);

            var categorias = await _categoriaRepository.ListarCategoriasAsync();
            if (categorias.Any(c => c.Codigo == categoria.Codigo))
                return ReglaIncumplida<object>("El código ya existe", "RegistrarCategoriaAsync", categoria?.Id_Categoria);

            if (categorias.Any(c => c.Nombre_Categoria?.ToLower() == categoria.Nombre_Categoria?.ToLower()))
                return ReglaIncumplida<object>("El nombre ya existe", "RegistrarCategoriaAsync", categoria?.Id_Categoria);

            var result = await _categoriaRepository.RegistrarCategoriaAsync(categoria);
            if (result > 0)
                return new ApiResponse<object> { IsSuccess = true, Message = Mensajes.MESSAGE_REGISTER };

            return ReglaIncumplida<object>(Mensajes.MESSAGE_REGISTER_FAILLED, "RegistrarCategoriaAsync", categoria?.Id_Categoria);
        }

        public async Task<ApiResponse<object>> EditarCategoriaAsync(Categoria categoria)
        {
            if (categoria == null)
                return ReglaIncumplida<object>(Mensajes.MESSAGE_NULL, "EditarCategoriaAsync", categoria?.Id_Categoria);

            var validationResult = await _validator.ValidateAsync(categoria);
            if (!validationResult.IsValid)
                return ReglaIncumplida<object>(string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage)), "EditarCategoriaAsync", categoria?.Id_Categoria);

            var categoriaExistente = await _categoriaRepository.ObtenerCategoriaAsync(categoria.Id_Categoria);
            if (categoriaExistente == null)
                return ReglaIncumplida<object>(Mensajes.MESSAGE_QUERY_NOT_FOUND, "EditarCategoriaAsync", categoria?.Id_Categoria);

            var categorias = await _categoriaRepository.ListarCategoriasAsync();
            if (categorias.Any(c => c.Nombre_Categoria?.ToLower() == categoria.Nombre_Categoria?.ToLower() && c.Id_Categoria != categoria.Id_Categoria))
            {
                return ReglaIncumplida<object>("El nombre ya existe.", "EditarCategoriaAsync", categoria?.Id_Categoria);
            }

            var result = await _categoriaRepository.EditarCategoriaAsync(categoria);
            if (result > 0)
                return new ApiResponse<object> { IsSuccess = true, Message = Mensajes.MESSAGE_UPDATE };

            return ReglaIncumplida<object>(Mensajes.MESSAGE_UPDATE_FAILLED, "EditarCategoriaAsync", categoria?.Id_Categoria);
        }

        public async Task<ApiResponse<int>> EliminarCategoriaAsync(int id)
        {
            try
            {
                var existe = await _categoriaRepository.ObtenerCategoriaAsync(id);
                if (existe == null)
                {
                    return ReglaIncumplida<int>(Mensajes.MESSAGE_QUERY_NOT_FOUND, "EliminarCategoriaAsync", id);
                }

                var result = await _categoriaRepository.EliminarCategoriaAsync(id);

                if (result > 0)
                    return new ApiResponse<int> { IsSuccess = true, Message = Mensajes.MESSAGE_DELETE };

                return ReglaIncumplida<int>(Mensajes.MESSAGE_DELETE_FAILLED, "EliminarCategoriaAsync", id);
            }
            catch (SqlException ex) when (ex.Number is 547 or 50547) 
            {
                return ReglaIncumplida<int>("No se puede eliminar la categoría porque tiene productos asociados.", "EliminarCategoriaAsync", id);
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
