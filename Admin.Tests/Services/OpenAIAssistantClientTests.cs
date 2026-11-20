using System.Net;
using System.Text;
using System.Text.Json;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Utilities.IA;

namespace Admin.Tests.Services
{
    [TestClass]
    public class OpenAIAssistantClientTests
    {
        [TestMethod]
        public async Task GenerateAsync_EnviaLaSolicitudYExtraeTodoElTexto()
        {
            var handler = new StubHttpMessageHandler(async (request, cancellationToken) =>
            {
                Assert.AreEqual(HttpMethod.Post, request.Method);
                Assert.AreEqual(new Uri("https://api.openai.com/v1/responses"), request.RequestUri);
                Assert.AreEqual("Bearer test-api-key", request.Headers.Authorization?.ToString());

                using var body = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken));
                Assert.AreEqual("gpt-6-luna", body.RootElement.GetProperty("model").GetString());
                Assert.AreEqual("datos y pregunta", body.RootElement.GetProperty("input").GetString());
                Assert.IsFalse(body.RootElement.GetProperty("store").GetBoolean());

                return JsonResponse(
                    "{\"output\":[{\"type\":\"reasoning\"},{\"type\":\"message\",\"content\":[" +
                    "{\"type\":\"output_text\",\"text\":\"Hay buen movimiento.\"}," +
                    "{\"type\":\"output_text\",\"text\":\"Revisa el stock.\"}]}]}");
            });
            var client = CreateClient(handler);

            var result = await client.GenerateAsync("datos y pregunta");

            Assert.AreEqual("Hay buen movimiento." + Environment.NewLine + "Revisa el stock.", result);
        }

        [TestMethod]
        public async Task GenerateAsync_SinClave_FallaAntesDeEnviarLaSolicitud()
        {
            var handler = new StubHttpMessageHandler((_, _) =>
                throw new AssertFailedException("No debía enviarse una solicitud sin API key."));
            var client = CreateClient(handler, apiKey: null);

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => client.GenerateAsync("consulta"));
        }

        [TestMethod]
        public async Task GenerateAsync_AnteErrorHttp_LanzaExcepcion()
        {
            var handler = new StubHttpMessageHandler((_, _) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized)));
            var client = CreateClient(handler);

            await Assert.ThrowsExceptionAsync<HttpRequestException>(() => client.GenerateAsync("consulta"));
        }

        [TestMethod]
        public async Task GenerateAsync_SinTextoEnLaRespuesta_LanzaExcepcion()
        {
            var handler = new StubHttpMessageHandler((_, _) =>
                Task.FromResult(JsonResponse("{\"output\":[]}")));
            var client = CreateClient(handler);

            await Assert.ThrowsExceptionAsync<JsonException>(() => client.GenerateAsync("consulta"));
        }

        private static OpenAIAssistantClient CreateClient(HttpMessageHandler handler, string? apiKey = "test-api-key")
        {
            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://api.openai.com/v1/")
            };
            var settings = new Dictionary<string, string?>
            {
                ["OpenAI:ApiKey"] = apiKey,
                ["OpenAI:Model"] = "gpt-6-luna"
            };
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

            return new OpenAIAssistantClient(httpClient, configuration);
        }

        private static HttpResponseMessage JsonResponse(string json)
            => new(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

        private sealed class StubHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _send;

            public StubHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send)
            {
                _send = send;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
                => _send(request, cancellationToken);
        }
    }
}
