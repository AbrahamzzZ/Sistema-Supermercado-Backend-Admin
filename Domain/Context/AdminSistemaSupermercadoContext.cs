using Domain.Model;
using Domain.Model.Dto;
using Domain.Model.Dto.Negocio;
using Microsoft.EntityFrameworkCore;

namespace Domain.Context
{
    public class AdminSistemaSupermercadoContext : DbContext
    {
        public DbSet<Categoria> Categoria { get; set; }
        public DbSet<Cliente> Cliente { get; set; }
        public DbSet<Negocio> Negocio { get; set; }
        public DbSet<Oferta> Oferta { get; set; }
        public DbSet<Producto> Producto { get; set; }
        public DbSet<Proveedor> Proveedor { get; set; }
        public DbSet<Sucursal> Sucursal { get; set; }
        public DbSet<Transportista> Transportista { get; set; }
        public DbSet<ProductoCategoria> ProductosDto { get; set; }
        public DbSet<OfertaProducto> OfertasDto { get; set; }
        public DbSet<ProductoMasComprado> ProductoMasComprados { get; set; }
        public DbSet<ProductoMasVendido> ProductoMasVendidos { get; set; }
        public DbSet<TopCliente> TopClientes { get; set; }
        public DbSet<TopProveedor> TopProveedores { get; set; }
        public DbSet<ViajesTransportista> ViajesTransportistas { get; set; }
        public DbSet<EmpleadoProductivo> EmpleadoProductivos { get; set; }
        public DbSet<SucursalUbicacion> SucursalUbicacion { get; set; }

        public AdminSistemaSupermercadoContext(DbContextOptions<AdminSistemaSupermercadoContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductoCategoria>().HasNoKey().ToView(null);

            modelBuilder.Entity<OfertaProducto>().HasNoKey()
                .ToView(null)
                .Property(c => c.Descuento)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ProductoMasComprado>().HasNoKey().ToView(null);

            modelBuilder.Entity<ProductoMasVendido>().HasNoKey().ToView(null);

            modelBuilder.Entity<TopProveedor>().HasNoKey().ToView(null);

            modelBuilder.Entity<TopCliente>().HasNoKey().ToView(null);

            modelBuilder.Entity<ViajesTransportista>().HasNoKey().ToView(null);

            modelBuilder.Entity<EmpleadoProductivo>().HasNoKey().ToView(null);

            modelBuilder.Entity<SucursalUbicacion>().HasNoKey().ToView(null);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AdminSistemaSupermercadoContext).Assembly);
        }
    }
}
