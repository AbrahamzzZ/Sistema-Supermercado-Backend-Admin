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
    public class ClienteRepository : IClienteRepository
    {
        private readonly AdminSistemaSupermercadoContext _context;
        private readonly ICurrentUserService _currentUser;

        public ClienteRepository(AdminSistemaSupermercadoContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<List<Cliente>> ListarClientesAsync()
        {
            return await _context.Cliente
                .FromSqlRaw("EXEC PA_LISTA_CLIENTE")
                .ToListAsync();
        }

        public async Task<Paginacion<Cliente>> ListarClientesPaginacionAsync(int pageNumber, int pageSize)
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "PA_LISTA_CLIENTE_PAGINACION";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@PageNumber", pageNumber));
            command.Parameters.Add(new SqlParameter("@PageSize", pageSize));

            await _context.Database.OpenConnectionAsync();

            var clientes = new List<Cliente>();
            int totalCount = 0;

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    clientes.Add(reader.LeerAuditoria(new Cliente
                    {
                        Id_Cliente = reader.GetInt32(0),
                        Codigo = reader.GetString(1),
                        Nombres = reader.GetString(2),
                        Apellidos = reader.GetString(3),
                        Cedula = reader.GetString(4),
                        Telefono = reader.GetString(5),
                        Correo_Electronico = reader.GetString(6)
                    }));
                }

                if (await reader.NextResultAsync() && await reader.ReadAsync())
                {
                    totalCount = reader.GetInt32(0);
                }
            }


            return new Paginacion<Cliente> { Items = clientes, TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize };
        }

        public async Task<Cliente?> ObtenerClienteAsync(int idCliente)
        {
            var idParam = new SqlParameter("@Id_Cliente", idCliente);
            return await Task.Run(() =>
                _context.Cliente
                .FromSqlRaw("EXEC PA_OBTENER_CLIENTE @Id_Cliente = @Id_Cliente", idParam)
                .AsNoTracking()
                .AsEnumerable()
                .FirstOrDefault()
            );
        }

        public async Task<int> RegistrarClienteAsync(Cliente cliente)
        {
            return await _context.Database.ExecuteSqlRawAsync(
                "EXEC PA_REGISTRAR_CLIENTE @Codigo = @Codigo, @Nombres = @Nombres, @Apellidos = @Apellidos, @Cedula = @Cedula, @Telefono = @Telefono, @Correo_Electronico = @Correo_Electronico, @Id_Usuario_Creacion = @Id_Usuario_Creacion",
                new SqlParameter("@Codigo", cliente.Codigo ?? (object)DBNull.Value),
                new SqlParameter("@Nombres", cliente.Nombres ?? (object)DBNull.Value),
                new SqlParameter("@Apellidos", cliente.Apellidos ?? (object)DBNull.Value),
                new SqlParameter("@Cedula", cliente.Cedula ?? (object)DBNull.Value),
                new SqlParameter("@Telefono", cliente.Telefono ?? (object)DBNull.Value),
                new SqlParameter("@Correo_Electronico", cliente.Correo_Electronico ?? (object)DBNull.Value),
                new SqlParameter("@Id_Usuario_Creacion", _currentUser.IdUsuario.ComoParametro())
            );
        }

        public async Task<int> EditarClienteAsync(Cliente cliente)
        {
            return await _context.Database.ExecuteSqlRawAsync(
                "EXEC PA_EDITAR_CLIENTE @Id_Cliente = @Id_Cliente, @Nombres = @Nombres, @Apellidos = @Apellidos, @Cedula = @Cedula, @Telefono = @Telefono, @Correo_Electronico = @Correo_Electronico, @Id_Usuario_Modificacion = @Id_Usuario_Modificacion",
                new SqlParameter("@Id_Cliente", cliente.Id_Cliente),
                new SqlParameter("@Nombres", cliente.Nombres ?? (object)DBNull.Value),
                new SqlParameter("@Apellidos", cliente.Apellidos ?? (object)DBNull.Value),
                new SqlParameter("@Cedula", cliente.Cedula ?? (object)DBNull.Value),
                new SqlParameter("@Telefono", cliente.Telefono ?? (object)DBNull.Value),
                new SqlParameter("@Correo_Electronico", cliente.Correo_Electronico ?? (object)DBNull.Value),
                new SqlParameter("@Id_Usuario_Modificacion", _currentUser.IdUsuario.ComoParametro())
            );
        }

        public async Task<int> EliminarClienteAsync(int idCliente)
        {
            return await _context.Database.ExecuteSqlRawAsync(
                "EXEC PA_ELIMINAR_CLIENTE @Id_Cliente = @Id_Cliente, @Id_Usuario_Modificacion = @Id_Usuario_Modificacion",
                new SqlParameter("@Id_Cliente", idCliente),
                new SqlParameter("@Id_Usuario_Modificacion", _currentUser.IdUsuario.ComoParametro())
            );
        }
    }
}
