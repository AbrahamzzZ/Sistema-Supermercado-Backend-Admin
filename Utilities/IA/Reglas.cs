using System.Text.RegularExpressions;

namespace Utilities.IA
{
    public static class Reglas
    {
        private static readonly Regex RegexVendido = new(@"\b(vendid[oa]s?|ventas?|facturaci[oó]n|ingresos?)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex RegexComprado = new(@"\b(comprad[oa]s?|compras?|abastecimiento|adquisiciones?)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex RegexProducto = new(@"\b(producto|productos|art[ií]culo|art[ií]culos|item|items|mercanc[ií]a)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex RegexCategoria = new(@"\b(categor[ií]a|categor[ií]as|rubro|rubros|secci[oó]n|secciones|departamento)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex RegexCliente = new(@"\b(cliente|clientes|comprador|compradores|consumidor|consumidores|frecuentes)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex RegexProveedor = new(@"\b(proveedor|proveedores|distribuidor|distribuidores)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex RegexSucursal = new(@"\b(sucursal|sucursales|tienda|tiendas|local|locales)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static TipoAnalisis DetectarTipoAnalisis(string? prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                return TipoAnalisis.Invalido;

            prompt = prompt.ToLower();

            if (RegexProducto.IsMatch(prompt)) return TipoAnalisis.Producto;
            if (RegexCategoria.IsMatch(prompt)) return TipoAnalisis.Categoria;
            if (RegexCliente.IsMatch(prompt)) return TipoAnalisis.Cliente;
            if (RegexProveedor.IsMatch(prompt)) return TipoAnalisis.Proveedor;
            if (RegexSucursal.IsMatch(prompt)) return TipoAnalisis.Sucursal;
            if (RegexVendido.IsMatch(prompt)) return TipoAnalisis.Vendido;
            if (RegexComprado.IsMatch(prompt)) return TipoAnalisis.Comprado;

            return TipoAnalisis.Invalido;
        }
    }

    public enum TipoAnalisis
    {
        Vendido,
        Comprado,
        Producto,
        Categoria,
        Cliente,
        Proveedor,
        Sucursal,
        Invalido
    }
}
