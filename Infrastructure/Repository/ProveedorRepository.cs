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
    public class ProveedorRepository : IProveedorRepository
    {
        private readonly AdminSistemaSupermercadoContext _context;
        private readonly ICurrentUserService _currentUser;

        public ProveedorRepository(AdminSistemaSupermercadoContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<List<Proveedor>> ListarProveedoresAsync()
        {
            return await _context.Proveedor
                .FromSqlRaw("EXEC PA_LISTA_PROVEEDOR")
                .ToListAsync();
        }

        public async Task<Paginacion<Proveedor>> ListarProveedoresPaginacionAsync(int pageNumber, int pageSize)
        {

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "PA_LISTA_PROVEEDOR_PAGINACION";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@PageNumber", pageNumber));
            command.Parameters.Add(new SqlParameter("@PageSize", pageSize));

            await _context.Database.OpenConnectionAsync();

            var proveedores = new List<Proveedor>();
            int totalCount = 0;

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    proveedores.Add(reader.LeerAuditoria(new Proveedor
                    {
                        Id_Proveedor = reader.GetInt32(0),
                        Codigo = reader.GetString(1),
                        Nombres = reader.GetString(2),
                        Apellidos = reader.GetString(3),
                        Cedula = reader.GetString(4),
                        Telefono = reader.GetString(5),
                        Correo_Electronico = reader.GetString(6),
                        Estado = reader.GetBoolean(7)
                    }));
                }

                if (await reader.NextResultAsync() && await reader.ReadAsync())
                {
                    totalCount = reader.GetInt32(0);
                }
            }

            return new Paginacion<Proveedor> { Items = proveedores, TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize };
        }

        public async Task<Proveedor?> ObtenerProveedorAsync(int idProveedor)
        {
            var idParam = new SqlParameter("@Id_Proveedor", idProveedor);
            return await Task.Run(() =>
                _context.Proveedor
                .FromSqlRaw("EXEC PA_OBTENER_PROVEEDOR @Id_Proveedor = @Id_Proveedor", idParam)
                .AsNoTracking()
                .AsEnumerable()
                .FirstOrDefault()
            );
        }

        public async Task<int> RegistrarProveedorAsync(Proveedor proveedor)
        {
            return await _context.Database.ExecuteSqlRawAsync(
                "EXEC PA_REGISTRAR_PROVEEDOR @Codigo = @Codigo, @Nombres = @Nombres, @Apellidos = @Apellidos, @Cedula = @Cedula, @Telefono = @Telefono, @Correo_Electronico = @Correo_Electronico, @Estado = @Estado, @Id_Usuario_Creacion = @Id_Usuario_Creacion",
                new SqlParameter("@Codigo", proveedor.Codigo ?? (object)DBNull.Value),
                new SqlParameter("@Nombres", proveedor.Nombres ?? (object)DBNull.Value),
                new SqlParameter("@Apellidos", proveedor.Apellidos ?? (object)DBNull.Value),
                new SqlParameter("@Cedula", proveedor.Cedula ?? (object)DBNull.Value),
                new SqlParameter("@Telefono", proveedor.Telefono ?? (object)DBNull.Value),
                new SqlParameter("@Correo_Electronico", proveedor.Correo_Electronico ?? (object)DBNull.Value),
                new SqlParameter("@Estado", proveedor.Estado ?? (object)DBNull.Value),
                new SqlParameter("@Id_Usuario_Creacion", _currentUser.IdUsuario.ComoParametro())
            );
        }

        public async Task<int> EditarProveedorAsync(Proveedor proveedor)
        {
            return await _context.Database.ExecuteSqlRawAsync(
                "EXEC PA_EDITAR_PROVEEDOR @Id_Proveedor = @Id_Proveedor, @Nombres = @Nombres, @Apellidos = @Apellidos, @Cedula = @Cedula, @Telefono = @Telefono, @Correo_Electronico = @Correo_Electronico, @Estado = @Estado, @Id_Usuario_Modificacion = @Id_Usuario_Modificacion",
                new SqlParameter("@Id_Proveedor", proveedor.Id_Proveedor),
                new SqlParameter("@Nombres", proveedor.Nombres ?? (object)DBNull.Value),
                new SqlParameter("@Apellidos", proveedor.Apellidos ?? (object)DBNull.Value),
                new SqlParameter("@Cedula", proveedor.Cedula ?? (object)DBNull.Value),
                new SqlParameter("@Telefono", proveedor.Telefono ?? (object)DBNull.Value),
                new SqlParameter("@Correo_Electronico", proveedor.Correo_Electronico ?? (object)DBNull.Value),
                new SqlParameter("@Estado", proveedor.Estado ?? (object)DBNull.Value),
                new SqlParameter("@Id_Usuario_Modificacion", _currentUser.IdUsuario.ComoParametro())
            );
        }

        public async Task<int> EliminarProveedorAsync(int idProveedor)
        {
            return await _context.Database.ExecuteSqlRawAsync(
                "EXEC PA_ELIMINAR_PROVEEDOR @Id_Proveedor = @Id_Proveedor, @Id_Usuario_Modificacion = @Id_Usuario_Modificacion",
                new SqlParameter("@Id_Proveedor", idProveedor),
                new SqlParameter("@Id_Usuario_Modificacion", _currentUser.IdUsuario.ComoParametro())
            );
        }
    }
}
