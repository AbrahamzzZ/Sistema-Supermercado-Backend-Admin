using Domain.Model.Common;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Domain.Model;

public class Oferta : EntidadAuditable, IEliminable
{
    public int Id_Oferta { get; set; }

    public string? Codigo { get; set; }

    public string? Nombre_Oferta { get; set; }

    public int? Id_Producto { get; set; }

    public string? Descripcion { get; set; }

    public DateOnly? Fecha_Inicio { get; set; }

    public DateOnly? Fecha_Fin { get; set; }

    public decimal? Descuento { get; set; }

    public bool? Estado { get; set; }

    /// <summary>Alias de compatibilidad: el JSON de la API mantiene "fecha_Creacion".</summary>
    [NotMapped]
    public DateTime? Fecha_Creacion => FechaCreacion;

    [JsonIgnore]
    public bool Eliminado { get; set; }


    public virtual Producto? IdProductoNavigation { get; set; }
}
