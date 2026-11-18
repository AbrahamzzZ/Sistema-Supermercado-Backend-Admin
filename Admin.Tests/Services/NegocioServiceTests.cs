using Domain.Model;
using Domain.Model.Dto.Negocio;
using FluentValidation;
using Infrastructure.Repository.InterfacesRepository;
using Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Utilities.IA;
using Utilities.Shared;

namespace Admin.Tests.Services
{
    [TestClass]
    public class NegocioServiceTests
    {
        private Mock<INegocioRepository> _repositorio = null!;
        private Mock<IMicroserviciosApiClient> _microservicios = null!;
        private Mock<IOllamaClient> _ollama = null!;
        private NegocioService _servicio = null!;

        [TestInitialize]
        public void Inicializar()
        {
            _repositorio = new Mock<INegocioRepository>();
            _microservicios = new Mock<IMicroserviciosApiClient>();
            _ollama = new Mock<IOllamaClient>();
            _ollama.Setup(o => o.GenerateAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("respuesta de la IA");

            _servicio = new NegocioService(_repositorio.Object, Mock.Of<IValidator<Negocio>>(), _ollama.Object,
                _microservicios.Object, NullLogger<NegocioService>.Instance);
        }

        // ---------- Reportes (totales de Order + nombres de Admin) ----------

        [TestMethod]
        public async Task TopClientes_CombinaTotalesDeOrderConNombresDeAdmin()
        {
            _microservicios.Setup(m => m.ObtenerEstadisticaAsync("top-clientes"))
                .ReturnsAsync(new List<EstadisticaTotal> { new() { Id = 5, Total = 10 }, new() { Id = 9, Total = 3 } });
            _repositorio.Setup(r => r.ObtenerNombresClientesAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new Dictionary<int, string> { [5] = "Ana Pérez" });

            var resultado = await _servicio.ObtenerTopClientes();

            Assert.IsTrue(resultado.IsSuccess);
            Assert.AreEqual("Ana Pérez", resultado.Data![0].Nombre_Completo);
            Assert.AreEqual(10, resultado.Data[0].Compras_Totales);
            // El cliente 9 ya no existe en Admin: se muestra con un nombre genérico en vez de perder el dato.
            Assert.AreEqual("Cliente #9", resultado.Data[1].Nombre_Completo);
        }

        [TestMethod]
        public async Task Reporte_OrderNoResponde_AvisaQueElServicioNoEstaDisponible()
        {
            _microservicios.Setup(m => m.ObtenerEstadisticaAsync(It.IsAny<string>())).ReturnsAsync((List<EstadisticaTotal>?)null);

            var resultado = await _servicio.ObtenerProductoMasVendido();

            Assert.IsFalse(resultado.IsSuccess);
            Assert.AreEqual(Mensajes.MESSAGE_SERVICIO_NO_DISPONIBLE, resultado.Message);
            _repositorio.Verify(r => r.ObtenerNombresProductosAsync(It.IsAny<IEnumerable<int>>()), Times.Never);
        }

        [TestMethod]
        public async Task Reporte_SinMovimientos_DevuelveVacio()
        {
            _microservicios.Setup(m => m.ObtenerEstadisticaAsync(It.IsAny<string>())).ReturnsAsync(new List<EstadisticaTotal>());

            var resultado = await _servicio.ObtenerTopProveedores();

            Assert.IsFalse(resultado.IsSuccess);
            Assert.AreEqual(Mensajes.MESSAGE_QUERY_EMPTY, resultado.Message);
        }

        // ---------- Asistente de IA ----------

        [TestMethod]
        public async Task AnalisisIA_PreguntaAjenaAlNegocio_NoLlamaALaIA()
        {
            var resultado = await _servicio.AnalisisIA("¿Qué hora es en Tokio?");

            Assert.IsFalse(resultado.IsSuccess);
            Assert.AreEqual(Mensajes.MESSAGE_IA_FAILLED, resultado.Message);
            _ollama.Verify(o => o.GenerateAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task AnalisisIA_Productos_EnviaALaIAElStockDeInventory()
        {
            _repositorio.Setup(r => r.ObtenerProductosActivosAsync())
                .ReturnsAsync(new Dictionary<int, string> { [1] = "Arroz", [2] = "Azúcar" });
            _microservicios.Setup(m => m.ObtenerStocksAsync())
                .ReturnsAsync(new List<StockProducto> { new() { IdProducto = 1, Stock = 12 } });

            var resultado = await _servicio.AnalisisIA("¿Qué productos tienen poco stock?");

            Assert.IsTrue(resultado.IsSuccess);
            Assert.AreEqual("respuesta de la IA", resultado.Data);
            _ollama.Verify(o => o.GenerateAsync(
                It.Is<string>(p => p.Contains("Arroz: 12 unidades en stock") && p.Contains("Azúcar: 0 unidades en stock")),
                It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task AnalisisIA_InventoryNoResponde_NoLlamaALaIA()
        {
            _repositorio.Setup(r => r.ObtenerProductosActivosAsync()).ReturnsAsync(new Dictionary<int, string> { [1] = "Arroz" });
            _microservicios.Setup(m => m.ObtenerStocksAsync()).ReturnsAsync((List<StockProducto>?)null);

            var resultado = await _servicio.AnalisisIA("¿Qué productos tienen poco stock?");

            Assert.IsFalse(resultado.IsSuccess);
            Assert.AreEqual(Mensajes.MESSAGE_SERVICIO_NO_DISPONIBLE, resultado.Message);
            _ollama.Verify(o => o.GenerateAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task AnalisisIA_OllamaCaido_DevuelveMensajeControlado()
        {
            _repositorio.Setup(r => r.ObtenerNombresCategoriasAsync()).ReturnsAsync(new List<string> { "Bebidas" });
            _ollama.Setup(o => o.GenerateAsync(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new HttpRequestException("Ollama apagado"));

            var resultado = await _servicio.AnalisisIA("¿Cuántas categorías tengo?");

            Assert.IsFalse(resultado.IsSuccess);
            Assert.AreEqual(Mensajes.MESSAGE_IA_NO_DISPONIBLE, resultado.Message);
        }
    }
}
