namespace Domain.Model.Dto
{
    public class LogSistema
    {
        public string Id_Log { get; set; } = "";
        
        public string? Codigo { get; set; }
        public string? Mensaje { get; set; }
        public string? Detalle { get; set; }
        public int? Id_Usuario { get; set; }
        public DateTimeOffset Fecha { get; set; }
        public string? Endpoint { get; set; }
        public string? Metodo { get; set; }
        public string? Nivel { get; set; }
        public string? Servicio { get; set; }
    }
}
