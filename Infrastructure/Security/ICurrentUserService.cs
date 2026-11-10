namespace Infrastructure.Security
{
    /// <summary>
    /// Usuario autenticado de la petición actual (sale del token JWT, nunca del body).
    /// Se usa para llenar las columnas de auditoría ID_USUARIO_CREACION / ID_USUARIO_MODIFICACION.
    /// </summary>
    public interface ICurrentUserService
    {
        int? IdUsuario { get; }
    }
}
