using System.Net.Http.Json;

namespace Utilities.IA
{
    public interface IOllamaClient
    {
        Task<string> GenerateAsync(string prompt, string model = "qwen3:8b");
    }

    public class OllamaClient : IOllamaClient
    {
        private readonly HttpClient _http;

        public OllamaClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<string> GenerateAsync(string prompt, string model = "qwen3:8b")
        {
            var body = new
            {
                model = model,
                prompt = prompt,
                stream = false
            };

            var response = await _http.PostAsJsonAsync("/api/generate", body);

            var result = await response.Content.ReadFromJsonAsync<OllamaResponse>();

            return result?.response ?? string.Empty;
        }
    }

    public class OllamaResponse
    {
        public string response { get; set; }
    }
}
