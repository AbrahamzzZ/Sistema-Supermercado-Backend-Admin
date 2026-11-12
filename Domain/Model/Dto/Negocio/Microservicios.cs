namespace Domain.Model.Dto.Negocio
{
    public class EstadisticaTotal
    {
        public int Id { get; set; }
        public int Total { get; set; }
    }

    public class EstadisticaTendencia
    {
        public int IdProducto { get; set; }
        public DateTime Fecha { get; set; }
        public int Cantidad { get; set; }
    }

    public class StockProducto
    {
        public int IdProducto { get; set; }
        public int Stock { get; set; }
    }

    public class UsuarioNombre
    {
        public string? Nombre_Completo { get; set; }
    }

    public class AnalisisIARequest
    {
        public string Prompt { get; set; } = "";
    }
}
