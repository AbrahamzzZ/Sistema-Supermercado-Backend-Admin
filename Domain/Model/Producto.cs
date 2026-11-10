using Domain.Model.Common;
using System.Text.Json.Serialization;

namespace Domain.Model;

public class Producto : EntidadAuditable, IEliminable
{
    public int Id_Producto { get; set; }

    public string? Codigo { get; set; }

    public string? Descripcion { get; set; }

    public string? Nombre_Producto { get; set; }

    public int? Id_Categoria { get; set; }

    public string? Pais_Origen { get; set; }

    public bool? Estado { get; set; }

    [JsonIgnore]
    public bool Eliminado { get; set; }


    public virtual Categoria? IdCategoriaNavigation { get; set; }


    public virtual ICollection<Oferta> Oferta { get; set; } = new List<Oferta>();
}
