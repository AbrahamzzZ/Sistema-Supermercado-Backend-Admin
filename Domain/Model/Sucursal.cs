using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Model;

public class Sucursal
{
    public int Id_Sucursal { get; set; }

    public string? Codigo { get; set; }

    public int? Id_Negocio { get; set; }

    public string? Nombre_Sucursal { get; set; }

    public string? Direccion_Sucursal { get; set; }

    public Point? Ubicacion_Sucursal { get; set; }

    public string? Ciudad_Sucursal { get; set; }

    public bool? Estado { get; set; }

    public virtual Negocio? IdNegocioNavigation { get; set; }

    [NotMapped]
    public int TotalCount { get; set; }
}
