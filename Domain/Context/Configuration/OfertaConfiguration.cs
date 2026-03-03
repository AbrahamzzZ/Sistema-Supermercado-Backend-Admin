using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Context.Configuration
{
    public class OfertaConfiguration : IEntityTypeConfiguration<Oferta>
    {
        public void Configure(EntityTypeBuilder<Oferta> builder)
        {
            builder.ToTable("OFERTA");
            builder.HasKey(e => e.Id_Oferta);

            builder.Property(e => e.Id_Oferta).HasColumnName("ID_OFERTA");
            builder.Property(e => e.Codigo)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("CODIGO");
            builder.Property(e => e.Descripcion)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasColumnName("DESCRIPCION");
            builder.Property(e => e.Descuento)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("DESCUENTO");
            builder.Property(e => e.Estado).HasColumnName("ESTADO");
            builder.Property(e => e.Fecha_Creacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("FECHA_CREACION");
            builder.Property(e => e.Fecha_Fin).HasColumnName("FECHA_FIN");
            builder.Property(e => e.Fecha_Inicio).HasColumnName("FECHA_INICIO");
            builder.Property(e => e.Id_Producto).HasColumnName("ID_PRODUCTO");
            builder.Property(e => e.Nombre_Oferta)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("NOMBRE_OFERTA");

            builder.HasOne(d => d.IdProductoNavigation)
                .WithMany(p => p.Oferta)
                .HasForeignKey(d => d.Id_Producto)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
