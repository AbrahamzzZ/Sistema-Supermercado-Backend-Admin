using Domain.Context;
using Domain.Model;
using Infrastructure.Repository.InterfacesRepository;
using Infrastructure.Security;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Utilities.Shared;

namespace Infrastructure.Repository
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly AdminSistemaSupermercadoContext _context;
        private readonly ICurrentUserService _currentUser;

        public CategoriaRepository(AdminSistemaSupermercadoContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
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
                    categorias.Add(reader.LeerAuditoria(new Categoria
                    {
                        Id_Categoria = reader.GetInt32(0),
                        Codigo = reader.GetString(1),
                        Nombre_Categoria = reader.GetString(2),
                        Estado = reader.GetBoolean(3)
                    }));
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
                .FromSqlRaw("EXEC PA_OBTENER_CATEGORIA @Id_Categoria = @Id_Categoria", idParam)
                .AsNoTracking()
                .AsEnumerable()
                .FirstOrDefault()
            );
        }

        public async Task<int> RegistrarCategoriaAsync(Categoria categoria)
        {
            return await _context.Database.ExecuteSqlRawAsync(
                "EXEC PA_REGISTRAR_CATEGORIA @Codigo = @Codigo, @Nombre = @Nombre, @Estado = @Estado, @Id_Usuario_Creacion = @Id_Usuario_Creacion",
                new SqlParameter("@Codigo", categoria.Codigo ?? (object)DBNull.Value),
                new SqlParameter("@Nombre", categoria.Nombre_Categoria ?? (object)DBNull.Value),
                new SqlParameter("@Estado", categoria.Estado ?? false),
                new SqlParameter("@Id_Usuario_Creacion", _currentUser.IdUsuario.ComoParametro())
            );
        }

        public async Task<int> EditarCategoriaAsync(Categoria categoria)
        {
            return await _context.Database.ExecuteSqlRawAsync(
                "EXEC PA_EDITAR_CATEGORIA @Id_Categoria = @Id_Categoria, @Nombre = @Nombre, @Estado = @Estado, @Id_Usuario_Modificacion = @Id_Usuario_Modificacion",
                new SqlParameter("@Id_Categoria", categoria.Id_Categoria),
                new SqlParameter("@Nombre", categoria.Nombre_Categoria ?? (object)DBNull.Value),
                new SqlParameter("@Estado", categoria.Estado ?? false),
                new SqlParameter("@Id_Usuario_Modificacion", _currentUser.IdUsuario.ComoParametro())
            );
        }

        public async Task<int> EliminarCategoriaAsync(int id)
        {
            return await _context.Database.ExecuteSqlRawAsync(
                "EXEC PA_ELIMINAR_CATEGORIA @Id_Categoria = @Id_Categoria, @Id_Usuario_Modificacion = @Id_Usuario_Modificacion",
                new SqlParameter("@Id_Categoria", id),
                new SqlParameter("@Id_Usuario_Modificacion", _currentUser.IdUsuario.ComoParametro())
            );
        }
    }
}
