using Domain.Context;
using Domain.Model;
using Infrastructure.Repository.InterfacesRepository;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Utilities.Shared;

namespace Infrastructure.Repository
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly AdminSistemaSupermercadoContext _context;

        public CategoriaRepository(AdminSistemaSupermercadoContext context)
        {
            _context = context;
        }

        public async Task<List<Categoria>> ListarCategoriasAsync()
        {
            return await _context.Categoria
                .FromSqlRaw("EXEC PA_LISTA_CATEGORIA")
                .ToListAsync();
        }

        public async Task<Paginacion<Categoria>> ListarCategoriasPaginacionAsync(int pageNumber, int pageSize)
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "PA_LISTA_CATEGORIA_PAGINACION";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@PageNumber", pageNumber));
            command.Parameters.Add(new SqlParameter("@PageSize", pageSize));

            await _context.Database.OpenConnectionAsync();

            var categorias = new List<Categoria>();
            int totalCount = 0;

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    categorias.Add(new Categoria
                    {
                        Id_Categoria = reader.GetInt32(0),
                        Codigo = reader.GetString(1),
                        Nombre_Categoria = reader.GetString(2),
                        Estado = reader.GetBoolean(3),
                        Fecha_Creacion = reader.GetDateTime(4)
                    });
                }

                if (await reader.NextResultAsync() && await reader.ReadAsync())
                {
                    totalCount = reader.GetInt32(0);
                }
            }

            return new Paginacion<Categoria> { Items = categorias, TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize };
        }

        public async Task<Categoria?> ObtenerCategoriaAsync(int idCategoria)
        {
            var idParam = new SqlParameter("@Id_Categoria", idCategoria);
            return await Task.Run(() => _context.Categoria
                .FromSqlRaw("EXEC PA_OBTENER_CATEGORIA @Id_Categoria", idParam)
                .AsNoTracking()
                .AsEnumerable()
                .FirstOrDefault()
            );
        }

        public async Task<int> RegistrarCategoriaAsync(Categoria categoria)
        {
            return await _context.Database.ExecuteSqlRawAsync(
                "EXEC PA_REGISTRAR_CATEGORIA @Codigo, @Nombre, @Estado",
                new SqlParameter("@Codigo", categoria.Codigo ?? (object)DBNull.Value),
                new SqlParameter("@Nombre", categoria.Nombre_Categoria ?? (object)DBNull.Value),
                new SqlParameter("@Estado", categoria.Estado ?? false)
            );
        }

        public async Task<int> EditarCategoriaAsync(Categoria categoria)
        {
            return await _context.Database.ExecuteSqlRawAsync(
                "EXEC PA_EDITAR_CATEGORIA @Id_Categoria, @Nombre, @Estado",
                new SqlParameter("@Id_Categoria", categoria.Id_Categoria),
                new SqlParameter("@Nombre", categoria.Nombre_Categoria ?? (object)DBNull.Value),
                new SqlParameter("@Estado", categoria.Estado ?? false)
            );
        }

        public async Task<int> EliminarCategoriaAsync(int id)
        {
            return await _context.Database.ExecuteSqlRawAsync(
                "EXEC PA_ELIMINAR_CATEGORIA @Id_Categoria",
                new SqlParameter("@Id_Categoria", id)
            );
        }
    }
}
