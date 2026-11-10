using Domain.Model.Common;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Domain.Model;

public class Cliente : EntidadAuditable, IEliminable
{
    public int Id_Cliente { get; set; }

    public string? Codigo { get; set; }

    public string? Nombres { get; set; }

    public string? Apellidos { get; set; }

    public string? Cedula { get; set; }

    public string? Telefono { get; set; }

    public string? Correo_Electronico { get; set; }

    /// <summary>Alias de compatibilidad: la columna FECHA_REGISTRO pasó a ser FECHA_CREACION (auditoría).</summary>
    [NotMapped]
    public DateTime? Fecha_Registro => FechaCreacion;

    [JsonIgnore]
    public bool Eliminado { get; set; }

    [NotMapped]
    public int TotalCount { get; set; }
}
