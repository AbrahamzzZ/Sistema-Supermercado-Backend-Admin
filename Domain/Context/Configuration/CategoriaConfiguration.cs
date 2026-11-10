using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Context.Configuration
{
    public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
    {
        public void Configure(EntityTypeBuilder<Categoria> builder)
        {
            builder.ToTable("CATEGORIA");
            builder.HasKey(e => e.Id_Categoria);

            builder.ConfigurarAuditoria();
            builder.ConfigurarEliminacionLogica();

            builder.Property(e => e.Id_Categoria).HasColumnName("ID_CATEGORIA");
            builder.Property(e => e.Codigo)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("CODIGO");
            builder.Property(e => e.Estado).HasColumnName("ESTADO");
            builder.Property(e => e.Nombre_Categoria)
                .HasMaxLength(50)
                .HasColumnName("NOMBRE_CATEGORIA");
        }
    }
}
