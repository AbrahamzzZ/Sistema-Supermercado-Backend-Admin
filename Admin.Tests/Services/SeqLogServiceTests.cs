using Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Net;
using System.Text;
using Utilities.Shared;

namespace Admin.Tests.Services
{
    [TestClass]
    public class SeqLogServiceTests
    {
        /// <summary>Simula la API HTTP de Seq y guarda las URLs que se le pidieron.</summary>
        private sealed class SeqFalso : HttpMessageHandler
        {
            public List<Uri> Peticiones { get; } = new();
            public int Total { get; set; }
            public string Eventos { get; set; } = "[]";
            public bool Caido { get; set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                if (Caido) throw new HttpRequestException("Seq no responde");
                Peticiones.Add(request.RequestUri!);

                var json = request.RequestUri!.AbsolutePath == "/api/data"
                    ? $$"""{"Columns":["Total"],"Rows":[[{{Total}}]]}"""
                    : Eventos;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });
            }
        }

        private SeqFalso _seq = null!;
        private SeqLogService _servicio = null!;

        [TestInitialize]
        public void Inicializar()
        {
            _seq = new SeqFalso();
            var factory = new Mock<IHttpClientFactory>();
            factory.Setup(f => f.CreateClient(SeqLogService.NombreHttpClient))
                   .Returns(() => new HttpClient(_seq) { BaseAddress = new Uri("http://seq") });
            _servicio = new SeqLogService(factory.Object, NullLogger<SeqLogService>.Instance);
        }

        private static string Evento(string id, string mensaje, string propiedadesExtra = "") => $$"""
            {"Id":"{{id}}","Timestamp":"2026-11-10T15:00:00-05:00","Level":"Error","RenderedMessage":"{{mensaje}}",
             "Exception":"System.Exception: boom",
             "Properties":[{"Name":"Servicio","Value":"Order"}{{propiedadesExtra}}]}
            """;

        private Uri PeticionEventos() => _seq.Peticiones.Single(u => u.AbsolutePath == "/api/events");
        private Uri PeticionConteo() => _seq.Peticiones.Single(u => u.AbsolutePath == "/api/data");

        [TestMethod]
        public async Task ListarPaginacion_SegundaPagina_PideLosEventosNecesariosYDevuelveSoloEsaPagina()
        {
            _seq.Total = 9;
            _seq.Eventos = "[" + string.Join(",", Enumerable.Range(1, 4).Select(i => Evento($"ev-{i}", $"error {i}"))) + "]";

            var resultado = await _servicio.ListarPaginacionAsync(pageNumber: 2, pageSize: 2, filtro: null, desde: null, hasta: null);

            Assert.IsTrue(resultado.IsSuccess);
            StringAssert.Contains(PeticionEventos().Query, "count=4");
            CollectionAssert.AreEqual(new[] { "ev-3", "ev-4" }, resultado.Data!.Items!.Select(l => l.Id_Log).ToArray());
            Assert.AreEqual(9, resultado.Data.TotalCount);
        }

        [TestMethod]
        public async Task ListarPaginacion_MapeaLasPropiedadesDeSerilog()
        {
            _seq.Total = 1;
            _seq.Eventos = "[" + Evento("ev-1", "Venta rechazada",
                """,{"Name":"CorrelationId","Value":"abc123"},{"Name":"IdUsuario","Value":7},{"Name":"RequestPath","Value":"/api/Venta"},{"Name":"RequestMethod","Value":"POST"}""") + "]";

            var log = (await _servicio.ListarPaginacionAsync(1, 10, null, null, null)).Data!.Items!.Single();

            Assert.AreEqual("Venta rechazada", log.Mensaje);
            Assert.AreEqual("abc123", log.Codigo);
            Assert.AreEqual(7, log.Id_Usuario);
            Assert.AreEqual("/api/Venta", log.Endpoint);
            Assert.AreEqual("POST", log.Metodo);
            Assert.AreEqual("Order", log.Servicio);
            Assert.AreEqual("Error", log.Nivel);
            Assert.AreEqual("System.Exception: boom", log.Detalle);
        }

        [TestMethod]
        public async Task ListarPaginacion_SoloErroresYWarningsDeLaAplicacion()
        {
            await _servicio.ListarPaginacionAsync(1, 10, null, null, null);

            var filtro = Uri.UnescapeDataString(PeticionEventos().Query);
            StringAssert.Contains(filtro, "@Level = 'Error' or @Level = 'Warning'");
            StringAssert.Contains(filtro, "StartsWith(SourceContext, 'Microsoft')");
        }

        [TestMethod]
        public async Task ListarPaginacion_TextoConComilla_SeEscapaParaNoRomperElFiltro()
        {
            await _servicio.ListarPaginacionAsync(1, 10, "O'Brien", null, null);

            var filtro = Uri.UnescapeDataString(PeticionEventos().Query);
            StringAssert.Contains(filtro, "Contains(@Message, 'O''Brien') ci");
        }

        [TestMethod]
        public async Task ListarPaginacion_RangoDeFechas_SeConvierteDeHoraEcuadorAUtc()
        {
            var dia = new DateOnly(2026, 11, 1);

            await _servicio.ListarPaginacionAsync(1, 10, null, dia, dia);

            // 01/11 00:00 en Ecuador (UTC-5) = 05:00 UTC; "hasta" incluye todo el día (02/11 05:00 UTC).
            var query = PeticionEventos().Query;
            StringAssert.Contains(query, "fromDateUtc=2026-11-01T05:00:00Z");
            StringAssert.Contains(query, "toDateUtc=2026-11-02T05:00:00Z");
            StringAssert.Contains(PeticionConteo().Query, "rangeStartUtc=2026-11-01T05:00:00Z");
        }

        [TestMethod]
        public async Task ListarPaginacion_SeqCaido_DevuelveMensajeControlado()
        {
            _seq.Caido = true;

            var resultado = await _servicio.ListarPaginacionAsync(1, 10, null, null, null);

            Assert.IsFalse(resultado.IsSuccess);
            Assert.AreEqual(Mensajes.MESSAGE_LOGS_NO_DISPONIBLES, resultado.Message);
        }
    }
}
