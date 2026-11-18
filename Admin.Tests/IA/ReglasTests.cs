using Utilities.IA;

namespace Admin.Tests.IA
{
    [TestClass]
    public class ReglasTests
    {
        [DataTestMethod]
        [DataRow("¿Qué productos tienen poco stock?", TipoAnalisis.Producto)]
        [DataRow("¿Cuántas categorías tengo?", TipoAnalisis.Categoria)]
        [DataRow("¿Quiénes son mis clientes frecuentes?", TipoAnalisis.Cliente)]
        [DataRow("¿Cuál es mi mejor proveedor?", TipoAnalisis.Proveedor)]
        [DataRow("¿Qué sucursal rinde más?", TipoAnalisis.Sucursal)]
        [DataRow("¿Cómo van las VENTAS este mes?", TipoAnalisis.Vendido)]
        [DataRow("Resumen del abastecimiento de la semana", TipoAnalisis.Comprado)]
        public void DetectarTipoAnalisis_PreguntaDelNegocio_DevuelveElTipo(string prompt, TipoAnalisis esperado)
        {
            Assert.AreEqual(esperado, Reglas.DetectarTipoAnalisis(prompt));
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow("¿Qué hora es en Tokio?")]
        public void DetectarTipoAnalisis_PreguntaAjenaOVacia_EsInvalida(string? prompt)
        {
            Assert.AreEqual(TipoAnalisis.Invalido, Reglas.DetectarTipoAnalisis(prompt));
        }

        [TestMethod]
        public void DetectarTipoAnalisis_ProductoTienePrioridadSobreVentas()
        {
            Assert.AreEqual(TipoAnalisis.Producto, Reglas.DetectarTipoAnalisis("productos más vendidos"));
        }
    }
}
