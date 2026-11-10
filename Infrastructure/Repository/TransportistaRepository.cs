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
    public class TransportistaRepository : ITransportistaRepository
    {
        private readonly AdminSistemaSupermercadoContext _context;
        private readonly ICurrentUserService _currentUser;

        public TransportistaRepository(AdminSistemaSupermercadoContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<List<Transportista>> ListarTransportistasAsync()
        {
            return await _context.Transportista
                .FromSqlRaw("EXEC PA_LISTA_TRANSPORTISTA")
                .ToListAsync();
        }

        public async Task<Paginacion<Transportista>> ListarTransportistasPaginacionAsync(int pageNumber, int pageSize)
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "PA_LISTA_TRANSPORTISTA_PAGINACION";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@PageNumber", pageNumber));
            command.Parameters.Add(new SqlParameter("@PageSize", pageSize));

            await _context.Database.OpenConnectionAsync();

            var transportistas = new List<Transportista>();
            int totalCount = 0;


            using (var reader = await command.ExecuteReaderAsync())
            {

                while (await reader.ReadAsync())
                {
                    byte[]? imagen = null;

                    if (!await reader.IsDBNullAsync(7))
                    {
                        long length = reader.GetBytes(7, 0, null, 0, 0);
                        imagen = new byte[length];
                        reader.GetBytes(7, 0, imagen, 0, (int)length);
                    }

                    transportistas.Add(reader.LeerAuditoria(new Transportista
                    {
                        Id_Transportista = reader.GetInt32(0),
                        Codigo = reader.GetString(1),
                        Nombres = reader.GetString(2),
                        Apellidos = reader.GetString(3),
                        Cedula = reader.GetString(4),
                        Telefono = reader.GetString(5),
                        Correo_Electronico = reader.GetString(6),
                        Imagen = imagen,
                        Estado = reader.GetBoolean(8)
                    }));
                }

                if (await reader.NextResultAsync() && await reader.ReadAsync())
                {
                    totalCount = reader.GetInt32(0);
                }
            }

            return new Paginacion<Transportista> { Items = transportistas, TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize };
        }

        public async Task<Transportista?> ObtenerTransportistaAsync(int idTranportista)
        {
            var idParam = new SqlParameter("@Id_Transportista", idTranportista);
            return await Task.Run(() =>
                _context.Transportista
                .FromSqlRaw("EXEC PA_OBTENER_TRANSPORTISTA @Id_Transportista = @Id_Transportista", idParam)
                .AsNoTracking()
                .AsEnumerable()
                .FirstOrDefault()
            );
        }

        public async Task<int> RegistrarTransportistaAsync(Transportista transportista)
        {
            ConvertirImagen(transportista);

            return await _context.Database.ExecuteSqlRawAsync(
                "EXEC PA_REGISTRAR_TRANSPORTISTA @Codigo = @Codigo, @Nombres = @Nombres, @Apellidos = @Apellidos, @Cedula = @Cedula, @Telefono = @Telefono, @Correo_Electronico = @Correo_Electronico, @Imagen = @Imagen, @Estado = @Estado, @Id_Usuario_Creacion = @Id_Usuario_Creacion",
                new SqlParameter("@Codigo", transportista.Codigo ?? (object)DBNull.Value),
                new SqlParameter("@Nombres", transportista.Nombres ?? (object)DBNull.Value),
                new SqlParameter("@Apellidos", transportista.Apellidos ?? (object)DBNull.Value),
                new SqlParameter("@Cedula", transportista.Cedula ?? (object)DBNull.Value),
                new SqlParameter("@Telefono", transportista.Telefono ?? (object)DBNull.Value),
                new SqlParameter("@Correo_Electronico", transportista.Correo_Electronico ?? (object)DBNull.Value),
                new SqlParameter("@Imagen", SqlDbType.VarBinary, -1) { Value = transportista.Imagen ?? (object)DBNull.Value },
                new SqlParameter("@Estado", transportista.Estado ?? (object)DBNull.Value),
                new SqlParameter("@Id_Usuario_Creacion", _currentUser.IdUsuario.ComoParametro())
            );
        }

        public async Task<int> EditarTransportistaAsync(Transportista transportista)
        {
            ConvertirImagen(transportista);

            return await _context.Database.ExecuteSqlRawAsync(
                "EXEC PA_EDITAR_TRANSPORTISTA @Id_Transportista = @Id_Transportista, @Nombres = @Nombres, @Apellidos = @Apellidos, @Cedula = @Cedula, @Telefono = @Telefono, @Correo_Electronico = @Correo_Electronico, @Imagen = @Imagen, @Estado = @Estado, @Id_Usuario_Modificacion = @Id_Usuario_Modificacion",
                new SqlParameter("@Id_Transportista", transportista.Id_Transportista),
                new SqlParameter("@Nombres", transportista.Nombres ?? (object)DBNull.Value),
                new SqlParameter("@Apellidos", transportista.Apellidos ?? (object)DBNull.Value),
                new SqlParameter("@Cedula", transportista.Cedula ?? (object)DBNull.Value),
                new SqlParameter("@Telefono", transportista.Telefono ?? (object)DBNull.Value),
                new SqlParameter("@Correo_Electronico", transportista.Correo_Electronico ?? (object)DBNull.Value),
                new SqlParameter("@Imagen", SqlDbType.VarBinary, -1) { Value = transportista.Imagen ?? (object)DBNull.Value },
                new SqlParameter("@Estado", transportista.Estado ?? (object)DBNull.Value),
                new SqlParameter("@Id_Usuario_Modificacion", _currentUser.IdUsuario.ComoParametro())
            );
        }

        public async Task<int> EliminarTransportistaAsync(int idTransportista)
        {
            return await _context.Database.ExecuteSqlRawAsync(
                "EXEC PA_ELIMINAR_TRANSPORTISTA @Id_Transportista = @Id_Transportista, @Id_Usuario_Modificacion = @Id_Usuario_Modificacion",
                new SqlParameter("@Id_Transportista", idTransportista),
                new SqlParameter("@Id_Usuario_Modificacion", _currentUser.IdUsuario.ComoParametro())
            );
        }

        private static void ConvertirImagen(Transportista transportista)
        {
            if (string.IsNullOrWhiteSpace(transportista.ImagenBase64))
                return;

            if (transportista.ImagenBase64.Contains(','))
                transportista.ImagenBase64 = transportista.ImagenBase64.Split(',')[1];

            transportista.Imagen = Convert.FromBase64String(transportista.ImagenBase64);
        }
    }
}
