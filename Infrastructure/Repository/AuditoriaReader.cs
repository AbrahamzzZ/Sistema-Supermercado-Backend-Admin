using Domain.Model.Common;
using System.Data.Common;

namespace Infrastructure.Repository
{
    /// <summary>
    /// Lee las columnas de auditoría por nombre (no por posición) en los SP de paginación leídos con DbDataReader.
    /// </summary>
    internal static class AuditoriaReader
    {
        public static T LeerAuditoria<T>(this DbDataReader reader, T entidad) where T : EntidadAuditable
        {
            entidad.IdUsuarioCreacion = LeerIntNullable(reader, "ID_USUARIO_CREACION");
            entidad.FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FECHA_CREACION"));
            entidad.IdUsuarioModificacion = LeerIntNullable(reader, "ID_USUARIO_MODIFICACION");
            var ordinalModificacion = reader.GetOrdinal("FECHA_MODIFICACION");
            entidad.FechaModificacion = reader.IsDBNull(ordinalModificacion) ? null : reader.GetDateTime(ordinalModificacion);
            return entidad;
        }

        private static int? LeerIntNullable(DbDataReader reader, string columna)
        {
            var ordinal = reader.GetOrdinal(columna);
            return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
        }

        /// <summary>Valor para un SqlParameter: el id del usuario o DBNull si no hay usuario autenticado.</summary>
        public static object ComoParametro(this int? idUsuario) => (object?)idUsuario ?? DBNull.Value;
    }
}
