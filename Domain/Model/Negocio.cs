using Domain.Model.Common;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Domain.Model;

public class Negocio : EntidadAuditable
{
    public int Id_Negocio { get; set; }

    public string? Nombre { get; set; }

    public string? Telefono { get; set; }

    public string? Ruc { get; set; }

    public string? Direccion { get; set; }

    public string? Correo_Electronico { get; set; }

    [NotMapped]
    public string? ImagenBase64 { get; set; }

    public byte[]? Logo { get; set; }


    public virtual ICollection<Sucursal> Sucursals { get; set; } = new List<Sucursal>();
}
