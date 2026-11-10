using Domain.Model.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Context.Configuration
{
    /// <summary>
    /// Mapeo común de las columnas de auditoría y de borrado lógico (mismo contrato en todos los microservicios).
    /// </summary>
    public static class AuditoriaConfiguration
    {
        public static EntityTypeBuilder<T> ConfigurarAuditoria<T>(this EntityTypeBuilder<T> builder) where T : EntidadAuditable
        {
            builder.Property(e => e.IdUsuarioCreacion).HasColumnName("ID_USUARIO_CREACION");
            builder.Property(e => e.FechaCreacion)
                .HasColumnName("FECHA_CREACION")
                .HasColumnType("datetime2")
                .HasDefaultValueSql("(sysutcdatetime())")
                .ValueGeneratedOnAdd();
            builder.Property(e => e.IdUsuarioModificacion).HasColumnName("ID_USUARIO_MODIFICACION");
            builder.Property(e => e.FechaModificacion)
                .HasColumnName("FECHA_MODIFICACION")
                .HasColumnType("datetime2");
            return builder;
        }

        public static EntityTypeBuilder<T> ConfigurarEliminacionLogica<T>(this EntityTypeBuilder<T> builder) where T : class, IEliminable
        {
            builder.Property(e => e.Eliminado)
                .HasColumnName("ELIMINADO")
                .HasDefaultValue(false);
            return builder;
        }
    }
}
