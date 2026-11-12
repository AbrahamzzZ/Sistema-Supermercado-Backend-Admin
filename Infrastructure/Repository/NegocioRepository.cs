using Domain.Context;
using Domain.Model;
using Domain.Model.Dto.Negocio;
using Infrastructure.Repository.InterfacesRepository;
using Microsoft.Data.SqlClient;
using Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Infrastructure.Repository
{
    public class NegocioRepository : INegocioRepository
    {
        private readonly AdminSistemaSupermercadoContext _context;
        private readonly ICurrentUserService _currentUser;

        public NegocioRepository(AdminSistemaSupermercadoContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<Negocio?> ObtenerNegocioAsync(int idNegocio)
        {
            var idParam = new SqlParameter("@Id_Negocio", idNegocio);
            return await Task.Run(() => _context.Negocio
                .FromSqlRaw("EXEC PA_OBTENER_NEGOCIO @Id_Negocio = @Id_Negocio", idParam)
                .AsNoTracking()
                .AsEnumerable()
                .FirstOrDefault()
            );
        }

        public async Task<int> EditarNegocioAsync(Negocio negocio)
        {
            if (!string.IsNullOrWhiteSpace(negocio.ImagenBase64))
            {
                if (negocio.ImagenBase64.Contains(","))
                {
                    negocio.ImagenBase64 = negocio.ImagenBase64.Split(',')[1];
                }

                negocio.Logo = Convert.FromBase64String(negocio.ImagenBase64);
            }

            return await _context.Database.ExecuteSqlRawAsync(
                "EXEC PA_EDITAR_NEGOCIO @Id_Negocio = @Id_Negocio, @Nombre = @Nombre, @Telefono = @Telefono, @Ruc = @Ruc, @Direccion = @Direccion, @Correo_Electronico = @Correo_Electronico, @Logo = @Logo, @Id_Usuario_Modificacion = @Id_Usuario_Modificacion",
                new SqlParameter("@Id_Negocio", negocio.Id_Negocio),
                new SqlParameter("@Nombre", negocio.Nombre ?? (object)DBNull.Value),
                new SqlParameter("@Telefono", negocio.Telefono ?? (object)DBNull.Value),
                new SqlParameter("@Ruc", negocio.Ruc ?? (object)DBNull.Value),
                new SqlParameter("@Direccion", negocio.Direccion ?? (object)DBNull.Value),
                new SqlParameter("@Correo_Electronico", negocio.Correo_Electronico ?? (object)DBNull.Value),
                new SqlParameter("@Logo", SqlDbType.VarBinary, -1) { Value = negocio.Logo ?? (object)DBNull.Value },
                new SqlParameter("@Id_Usuario_Modificacion", _currentUser.IdUsuario.ComoParametro())
            );
        }


        public async Task<Dictionary<int, string>> ObtenerNombresProductosAsync(IEnumerable<int> ids)
        {
            var lista = ids.Distinct().ToList();
            return await _context.Producto.AsNoTracking()
                .Where(p => lista.Contains(p.Id_Producto))
                .ToDictionaryAsync(p => p.Id_Producto, p => p.Nombre_Producto ?? "");
        }

        public async Task<Dictionary<int, string>> ObtenerNombresClientesAsync(IEnumerable<int> ids)
        {
            var lista = ids.Distinct().ToList();
            return await _context.Cliente.AsNoTracking()
                .Where(c => lista.Contains(c.Id_Cliente))
                .ToDictionaryAsync(c => c.Id_Cliente, c => $"{c.Nombres} {c.Apellidos}".Trim());
        }

        public async Task<Dictionary<int, string>> ObtenerNombresProveedoresAsync(IEnumerable<int> ids)
        {
            var lista = ids.Distinct().ToList();
            return await _context.Proveedor.AsNoTracking()
                .Where(p => lista.Contains(p.Id_Proveedor))
                .ToDictionaryAsync(p => p.Id_Proveedor, p => $"{p.Nombres} {p.Apellidos}".Trim());
        }

        public async Task<Dictionary<int, string>> ObtenerNombresTransportistasAsync(IEnumerable<int> ids)
        {
            var lista = ids.Distinct().ToList();
            return await _context.Transportista.AsNoTracking()
                .Where(t => lista.Contains(t.Id_Transportista))
                .ToDictionaryAsync(t => t.Id_Transportista, t => $"{t.Nombres} {t.Apellidos}".Trim());
        }

        public async Task<Dictionary<int, string>> ObtenerProductosActivosAsync()
        {
            return await _context.Producto.AsNoTracking()
                .Where(p => !p.Eliminado)
                .ToDictionaryAsync(p => p.Id_Producto, p => p.Nombre_Producto ?? "");
        }

        public async Task<List<string>> ObtenerNombresCategoriasAsync()
        {
            return await _context.Categoria.AsNoTracking()
                .Where(c => !c.Eliminado)
                .OrderBy(c => c.Nombre_Categoria)
                .Select(c => c.Nombre_Categoria ?? "")
                .ToListAsync();
        }
    }
}
