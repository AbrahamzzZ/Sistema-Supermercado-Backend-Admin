using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Context.Configuration
{
    public class TransportistaConfiguration : IEntityTypeConfiguration<Transportista>
    {
        public void Configure(EntityTypeBuilder<Transportista> builder)
        {
            builder.ToTable("TRANSPORTISTA");
            builder.HasKey(e => e.Id_Transportista);

            builder.ConfigurarAuditoria();
            builder.ConfigurarEliminacionLogica();

            builder.Property(e => e.Id_Transportista).HasColumnName("ID_TRANSPORTISTA");
            builder.Property(e => e.Apellidos)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("APELLIDOS");
            builder.Property(e => e.Cedula)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("CEDULA");
            builder.Property(e => e.Codigo)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("CODIGO");
            builder.Property(e => e.Correo_Electronico)
                .HasMaxLength(50)
                .HasColumnName("CORREO_ELECTRONICO");
            builder.Property(e => e.Estado).HasColumnName("ESTADO");
            builder.Property(e => e.Imagen).HasColumnName("IMAGEN");
            builder.Property(e => e.Nombres)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("NOMBRES");
            builder.Property(e => e.Telefono)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("TELEFONO");
        }
    }
}
