using Domain.Model.Common;
using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Domain.Model;

public class Sucursal : EntidadAuditable, IEliminable
{
    public int Id_Sucursal { get; set; }

    public string? Codigo { get; set; }

    public int? Id_Negocio { get; set; }

    public string? Nombre_Sucursal { get; set; }

    public string? Direccion_Sucursal { get; set; }

    public Point? Ubicacion_Sucursal { get; set; }

    public string? Ciudad_Sucursal { get; set; }

    public bool? Estado { get; set; }

    [JsonIgnore]
    public bool Eliminado { get; set; }


    public virtual Negocio? IdNegocioNavigation { get; set; }

    [NotMapped]
    public int TotalCount { get; set; }
}
