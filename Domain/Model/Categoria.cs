using Domain.Model.Common;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Domain.Model;

public class Categoria : EntidadAuditable, IEliminable
{
    public int Id_Categoria { get; set; }

    public string? Codigo { get; set; }

    public string? Nombre_Categoria { get; set; }

    public bool? Estado { get; set; }

    /// <summary>Alias de compatibilidad: el JSON de la API mantiene "fecha_Creacion".</summary>
    [NotMapped]
    public DateTime? Fecha_Creacion => FechaCreacion;

    [JsonIgnore]
    public bool Eliminado { get; set; }


    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();

    [NotMapped]
    public int TotalCount { get; set; }
}
