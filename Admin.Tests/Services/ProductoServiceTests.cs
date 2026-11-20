using Domain.Model;
using Domain.Model.Dto;
using Domain.Model.Dto.Negocio;
using Infrastructure.Repository.InterfacesRepository;
using Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Utilities.Shared;

namespace Admin.Tests.Services
{
    [TestClass]
    public class ProductoServiceTests
    {
        [TestMethod]
        public async Task ListarProductos_AgregaStockYPreciosDeInventory()
        {
            var producto = new ProductoCategoria { Id_Producto = 7, Nombre_Producto = "Arroz" };
            var repository = new Mock<IProductoRepository>();
            repository.Setup(r => r.ListarProductosAsync()).ReturnsAsync(new List<ProductoCategoria> { producto });
            var microservicios = new Mock<IMicroserviciosApiClient>();
            microservicios.Setup(m => m.ObtenerStocksAsync()).ReturnsAsync(new List<StockProducto>
            {
                new() { IdProducto = 7, Stock = 24, PrecioCompra = 1.25m, PrecioVenta = 1.75m }
            });
            var service = new ProductoService(
                repository.Object,
                microservicios.Object,
                Mock.Of<FluentValidation.IValidator<Producto>>(),
                NullLogger<ProductoService>.Instance);

            var result = await service.ListarProductosAsync();

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(24, result.Data![0].Stock);
            Assert.AreEqual(1.25m, result.Data[0].PrecioCompra);
            Assert.AreEqual(1.75m, result.Data[0].PrecioVenta);
        }

        [TestMethod]
        public async Task ListarProductosPaginacion_SinRegistroDeInventario_UsaStockCeroYPreciosNulos()
        {
            var repository = new Mock<IProductoRepository>();
            repository.Setup(r => r.ListarProductosPaginacionAsync(1, 5))
                .ReturnsAsync(new Paginacion<ProductoCategoria>
                {
                    Items = new List<ProductoCategoria> { new() { Id_Producto = 9, Nombre_Producto = "Fresa" } },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 5
                });
            var microservicios = new Mock<IMicroserviciosApiClient>();
            microservicios.Setup(m => m.ObtenerStocksAsync()).ReturnsAsync(new List<StockProducto>());
            var service = new ProductoService(
                repository.Object,
                microservicios.Object,
                Mock.Of<FluentValidation.IValidator<Producto>>(),
                NullLogger<ProductoService>.Instance);

            var result = await service.ListarProductosPaginacionAsync(1, 5);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(0, result.Data!.Items[0].Stock);
            Assert.IsNull(result.Data.Items[0].PrecioCompra);
            Assert.IsNull(result.Data.Items[0].PrecioVenta);
        }

        [TestMethod]
        public async Task ListarProductos_InventoryNoDisponible_DevuelveErrorEnLugarDeDatosIncompletos()
        {
            var repository = new Mock<IProductoRepository>();
            repository.Setup(r => r.ListarProductosAsync())
                .ReturnsAsync(new List<ProductoCategoria> { new() { Id_Producto = 7 } });
            var microservicios = new Mock<IMicroserviciosApiClient>();
            microservicios.Setup(m => m.ObtenerStocksAsync()).ReturnsAsync((List<StockProducto>?)null);
            var service = new ProductoService(
                repository.Object,
                microservicios.Object,
                Mock.Of<FluentValidation.IValidator<Producto>>(),
                NullLogger<ProductoService>.Instance);

            var result = await service.ListarProductosAsync();

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(Mensajes.MESSAGE_SERVICIO_NO_DISPONIBLE, result.Message);
            Assert.IsNull(result.Data);
        }
    }
}
